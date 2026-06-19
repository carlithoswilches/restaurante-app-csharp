using System;
using System.Windows.Forms;
using App.BLL.Servicios;
using App.DAL.Modelos;

namespace App.UI.Forms
{
    /// <summary>
    /// Formulario de Login.
    /// Construido completamente por código — sin diseñador visual.
    /// Controla intentos fallidos y cierra la app al tercer intento.
    /// </summary>
    public class FrmLogin : Form
    {
        // ── Controles del formulario ─────────────────────────────────────────
        private Label      lblTitulo;
        private Label      lblUsuario;
        private Label      lblContrasena;
        private TextBox    txtUsuario;
        private TextBox    txtContrasena;
        private Button     btnIngresar;
        private Label      lblIntentos;
        private ErrorProvider errorProvider; // muestra icono de error junto al control

        // ── Estado interno ───────────────────────────────────────────────────
        // Contador de intentos fallidos — máximo 3 según requerimiento
        private int _intentosFallidos = 0;
        private readonly UsuarioServicio _servicio;

        public FrmLogin()
        {
            _servicio = new UsuarioServicio();
            InicializarComponentes();
        }

        /// <summary>
        /// Crea y configura todos los controles del formulario por código.
        /// Equivale a lo que hace el diseñador de Visual Studio automáticamente.
        /// </summary>
        private void InicializarComponentes()
        {
            // ── Configuración del formulario ─────────────────────────────────
            this.Text            = "Restaurante — Iniciar Sesión";
            this.Size            = new System.Drawing.Size(380, 280);
            this.StartPosition   = FormStartPosition.CenterScreen; // centrado en pantalla
            this.FormBorderStyle = FormBorderStyle.FixedSingle;    // no redimensionable
            this.MaximizeBox     = false;

            // ── ErrorProvider ────────────────────────────────────────────────
            // Muestra un ícono rojo de error junto a un control específico
            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            // ── Título ───────────────────────────────────────────────────────
            lblTitulo = new Label
            {
                Text      = "SISTEMA DE RESTAURANTE",
                Font      = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location  = new System.Drawing.Point(50, 20),
                Size      = new System.Drawing.Size(280, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            // ── Label y TextBox Usuario ──────────────────────────────────────
            lblUsuario = new Label
            {
                Text     = "Usuario:",
                Location = new System.Drawing.Point(50, 70),
                Size     = new System.Drawing.Size(80, 20)
            };

            txtUsuario = new TextBox
            {
                Location    = new System.Drawing.Point(140, 68),
                Size        = new System.Drawing.Size(180, 25),
                MaxLength   = 50,     // límite de caracteres igual al campo en BD
                TabIndex    = 0       // orden de foco con la tecla Tab
            };

            // ToolTip: texto de ayuda que aparece al pasar el mouse sobre el control
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(txtUsuario, "Ingrese su nombre de usuario");

            // ── Label y TextBox Contraseña ───────────────────────────────────
            lblContrasena = new Label
            {
                Text     = "Contraseña:",
                Location = new System.Drawing.Point(50, 110),
                Size     = new System.Drawing.Size(80, 20)
            };

            txtContrasena = new TextBox
            {
                Location     = new System.Drawing.Point(140, 108),
                Size         = new System.Drawing.Size(180, 25),
                PasswordChar = '*',   // oculta el texto con asteriscos — requerimiento
                MaxLength    = 50,
                TabIndex     = 1
            };
            tooltip.SetToolTip(txtContrasena, "Ingrese su contraseña");

            // ── Botón Ingresar ───────────────────────────────────────────────
            btnIngresar = new Button
            {
                Text     = "Ingresar",
                Location = new System.Drawing.Point(140, 148),
                Size     = new System.Drawing.Size(180, 35),
                TabIndex = 2
            };
            // Suscripción al evento Click — cuando el usuario hace clic llama a Btn_Click
            btnIngresar.Click += new EventHandler(BtnIngresar_Click);

            // ── Label de intentos ────────────────────────────────────────────
            lblIntentos = new Label
            {
                Text      = "",
                ForeColor = System.Drawing.Color.Red,
                Location  = new System.Drawing.Point(50, 200),
                Size      = new System.Drawing.Size(280, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            // ── KeyPress en contraseña: Enter dispara el login ───────────────
            // Requerimiento explícito del proyecto
            txtContrasena.KeyPress += (sender, e) =>
            {
                // e.KeyChar contiene el carácter de la tecla presionada
                // (char)Keys.Return es el carácter Enter
                if (e.KeyChar == (char)Keys.Return)
                    BtnIngresar_Click(sender, e);
            };

            // ── Agregar controles al formulario ──────────────────────────────
            // Controls.Add registra cada control para que sea visible y funcional
            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblUsuario);
            this.Controls.Add(txtUsuario);
            this.Controls.Add(lblContrasena);
            this.Controls.Add(txtContrasena);
            this.Controls.Add(btnIngresar);
            this.Controls.Add(lblIntentos);
        }

        /// <summary>
        /// Evento Click del botón Ingresar.
        /// Valida campos, llama al servicio y maneja los intentos fallidos.
        /// </summary>
        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            // Limpia errores visuales anteriores
            errorProvider.Clear();

            string usuario    = txtUsuario.Text.Trim();//Trim Elimina espacion al inicio y final
            string contrasena = txtContrasena.Text;

            // ── Validación visual con ErrorProvider ──────────────────────────
            bool hayError = false;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                // SetError: muestra ícono de error junto al control con el mensaje
                errorProvider.SetError(txtUsuario, "El usuario es obligatorio");
                hayError = true;
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                errorProvider.SetError(txtContrasena, "La contraseña es obligatoria");
                hayError = true;
            }

            // Si hay errores de validación, no continúa
            if (hayError) return;

            // ── Llamada al servicio de autenticación ─────────────────────────
            Usuario usuarioAutenticado = _servicio.Autenticar(usuario, contrasena);

            if (usuarioAutenticado != null)
            {
                // Guarda el usuario en la sesión estática — accesible desde toda la app
                Sesion.UsuarioActual = usuarioAutenticado;

                // Oculta el login y abre el MDI
                this.Hide();
                FrmMDI mdi = new FrmMDI();

                // Cuando el MDI se cierre, también cierra el login y termina la app
                mdi.FormClosed += (s, args) => this.Close();
                mdi.Show();
            }
            else
            {
                // Incrementa el contador de intentos fallidos
                _intentosFallidos++;
                int intentosRestantes = 3 - _intentosFallidos;

                if (_intentosFallidos >= 3)
                {
                    // Requerimiento: máximo 3 intentos → cierra la aplicación
                    MessageBox.Show(
                        "Ha superado el número máximo de intentos. La aplicación se cerrará.",
                        "Acceso denegado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    Application.Exit();
                }
                else
                {
                    lblIntentos.Text = $"Credenciales incorrectas. Intentos restantes: {intentosRestantes}";
                    txtContrasena.Clear();
                    txtContrasena.Focus();
                }
            }
        }
    }
}