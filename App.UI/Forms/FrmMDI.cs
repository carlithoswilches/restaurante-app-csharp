using System;
using System.Windows.Forms;
using App.BLL.Servicios;
using WinTimer = System.Windows.Forms.Timer;

namespace App.UI.Forms
{
    /// Formulario MDI principal — contenedor de todos los subformularios.
    /// MDI (Multiple Document Interface) permite abrir varios formularios
    /// hijos dentro de un formulario padre sin salir de la aplicación.
    public class FrmMDI : Form
    {
        // ── Controles del MDI ────────────────────────────────────────────────
        private MenuStrip   menuStrip;    // barra de menús superior
        private StatusStrip statusStrip;  // barra de estado inferior
        private ToolStrip   toolStrip;    // barra de accesos directos
        private WinTimer reloj;      // actualiza la hora cada segundo

        // Etiquetas del StatusStrip — muestran info del usuario logueado
        private ToolStripStatusLabel lblUsuarioStatus;
        private ToolStripStatusLabel lblPerfilStatus;
        private ToolStripStatusLabel lblHoraStatus;

        public FrmMDI()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            // ── Configuración base del MDI ───────────────────────────────────
            this.Text           = "Sistema de Restaurante";
            this.IsMdiContainer = true;  // convierte este Form en contenedor MDI
            this.WindowState    = FormWindowState.Maximized;
            this.MinimumSize    = new System.Drawing.Size(900, 600);

            // ── MenuStrip ────────────────────────────────────────────────────
            // Barra de menús que organiza los módulos del sistema
            menuStrip = new MenuStrip();

            // Menú: Platos
            var mnuPlatos = new ToolStripMenuItem("🍽 Platos"); //forma mas corta de decir:ToolStripMenuItem mnuPlatos = new ToolStripMenuItem("🍽 Platos");
            var mnuGestionPlatos = new ToolStripMenuItem("Gestión de Platos");
            mnuGestionPlatos.Click += (s, e) => AbrirFormulario(new FrmPlatos());
            mnuPlatos.DropDownItems.Add(mnuGestionPlatos);

            // Menú: Mesas
            var mnuMesas = new ToolStripMenuItem("🪑 Mesas");
            var mnuGestionMesas = new ToolStripMenuItem("Gestión de Mesas");
            mnuGestionMesas.Click += (s, e) => AbrirFormulario(new FrmMesas());
            mnuMesas.DropDownItems.Add(mnuGestionMesas);

            // Menú: Pedidos
            var mnuPedidos = new ToolStripMenuItem("📋 Pedidos");
            var mnuGestionPedidos = new ToolStripMenuItem("Gestión de Pedidos");
            mnuGestionPedidos.Click += (s, e) => AbrirFormulario(new FrmPedidos());
            mnuPedidos.DropDownItems.Add(mnuGestionPedidos);

            // Menú: Clientes
            var mnuClientes = new ToolStripMenuItem("👤 Clientes");
            var mnuGestionClientes = new ToolStripMenuItem("Gestión de Clientes");
            mnuGestionClientes.Click += (s, e) => AbrirFormulario(new FrmClientes());
            mnuClientes.DropDownItems.Add(mnuGestionClientes);

            // Menú: Usuarios — solo visible para Administrador
            var mnuUsuarios = new ToolStripMenuItem("⚙ Usuarios");
            mnuUsuarios.Visible = Sesion.TienePerfil("Administrador");
            var mnuGestionUsuarios = new ToolStripMenuItem("Gestión de Usuarios");
            mnuGestionUsuarios.Click += (s, e) => AbrirFormulario(new FrmUsuarios());
            mnuUsuarios.DropDownItems.Add(mnuGestionUsuarios);

            // Menú: Cerrar Sesión
            var mnuSesion  = new ToolStripMenuItem("🔒 Sesión");
            var mnuCerrar  = new ToolStripMenuItem("Cerrar Sesión");
            mnuCerrar.Click += BtnCerrarSesion_Click;
            mnuSesion.DropDownItems.Add(mnuCerrar);

            // Menú: Reportes
            var mnuReportes = new ToolStripMenuItem("📊 Reportes");
            var mnuGestionReportes = new ToolStripMenuItem("Reportes e Importación");
            mnuGestionReportes.Click += (s, e) => AbrirFormulario(new FrmReportes());
            mnuReportes.DropDownItems.Add(mnuGestionReportes);

            // Agrega todos los menús al MenuStrip
            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                mnuPlatos, mnuMesas, mnuPedidos, mnuClientes, mnuUsuarios, mnuSesion,mnuReportes
            });

            // ── ToolStrip ────────────────────────────────────────────────────
            // Barra de accesos directos con botones rápidos
            toolStrip = new ToolStrip();

            var btnPlatos = new ToolStripButton("Platos");
            btnPlatos.Click += (s, e) => AbrirFormulario(new FrmPlatos());

            var btnCerrarSesion = new ToolStripButton("Cerrar Sesión");
            btnCerrarSesion.Click += BtnCerrarSesion_Click;

            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                btnPlatos,
                new ToolStripSeparator(),
                btnCerrarSesion
            });

            // ── StatusStrip ──────────────────────────────────────────────────
           
            // Barra inferior con info del usuario logueado y reloj
            statusStrip = new StatusStrip();

            lblUsuarioStatus = new ToolStripStatusLabel
            {
                // Sesion.UsuarioActual tiene el usuario que inició sesión
                Text    = $"Usuario: {Sesion.UsuarioActual?.NombreUsuario}",
                // Spring=true empuja los siguientes elementos hacia la derecha
                Spring  = false
            };

            lblPerfilStatus = new ToolStripStatusLabel
            {
                Text = $"Perfil: {Sesion.UsuarioActual?.NombrePerfil}"
            };

            // Separador visual entre etiquetas
            var separador = new ToolStripStatusLabel(" | ");

            lblHoraStatus = new ToolStripStatusLabel
            {
                Text   = DateTime.Now.ToString("HH:mm:ss"),
                Spring = true,  // ocupa el espacio restante — empuja hacia la derecha
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                lblUsuarioStatus, separador, lblPerfilStatus, lblHoraStatus
            });

            // ── Timer — actualiza el reloj cada segundo ──────────────────────
            // Requerimiento explícito: Timer en StatusStrip
            reloj          = new WinTimer();
            reloj.Interval = 1000; // 1000 milisegundos = 1 segundo
            reloj.Tick    += (s, e) =>
            {
                // Tick se dispara cada Interval milisegundos
                lblHoraStatus.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            reloj.Start(); // inicia el temporizador

            // ── Agregar controles al formulario ──────────────────────────────
            this.Controls.Add(menuStrip);
            this.Controls.Add(toolStrip);
            this.Controls.Add(statusStrip);
            this.MainMenuStrip = menuStrip; // le dice al Form cuál es el menú principal
        }

        /// <summary>
        /// Abre un formulario hijo dentro del MDI.
        /// Verifica que no esté ya abierto y lo maximiza automáticamente
        /// para evitar la superposición en cascada de Windows Forms.
        /// </summary>
        private void AbrirFormulario(Form formulario)
        {
            // Recorre los formularios hijos actualmente abiertos
            foreach (Form hijo in this.MdiChildren)
            {
                if (hijo.GetType() == formulario.GetType())
                {
                    hijo.BringToFront();
                    // Asegura que siga maximizado al traerlo al frente
                    hijo.WindowState = FormWindowState.Maximized;
                    formulario.Dispose();
                    return;
                }
            }

            formulario.MdiParent = this;

            // WindowState = Maximized hace que el formulario ocupe todo el
            // espacio disponible dentro del MDI, sin desplazamiento en cascada
            formulario.WindowState = FormWindowState.Maximized;

            formulario.Show();
        }

        /// <summary>
        /// Cierra la sesión: limpia Sesion.UsuarioActual y vuelve al Login.
        /// </summary>
        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show(
                "¿Desea cerrar la sesión?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacion == DialogResult.Yes)
            {
                reloj.Stop();          // detiene el timer antes de cerrar
                Sesion.CerrarSesion(); // limpia el usuario de la sesión estática
                this.Close();          // cierra el MDI — esto reactiva el Login
            }
        }

        /// <summary>
        /// Al cerrar el MDI también detiene el timer para liberar recursos.
        /// Override del evento FormClosing del Form base.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            reloj.Stop();
            base.OnFormClosing(e); // siempre llama al base para no romper el comportamiento
        }
    }
}