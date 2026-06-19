using System;
using System.Collections.Generic;
using System.Windows.Forms;
using App.BLL.Servicios;
using App.DAL.Modelos;

namespace App.UI.Forms
{
    /// <summary>
    /// Formulario de gestión de Usuarios — módulo M5.
    /// Solo accesible para perfil Administrador — el MDI controla la visibilidad.
    /// Permite crear, editar y eliminar usuarios del sistema.
    /// </summary>
    public class FrmUsuarios : Form
    {
        // ── Controles ────────────────────────────────────────────────────────
        private Label         lblTitulo;
        private Label         lblNombreUsuario;
        private Label         lblContrasena;
        private Label         lblNombreCompleto;
        private Label         lblPerfil;
        private Label         lblActivo;

        private TextBox       txtNombreUsuario;
        private TextBox       txtContrasena;      // PasswordChar='*'
        private TextBox       txtNombreCompleto;

        // ComboBox de perfiles cargado desde BD
        private ComboBox      cmbPerfil;
        private CheckBox      chkActivo;

        private Button        btnGuardar;
        private Button        btnEditar;
        private Button        btnEliminar;
        private Button        btnLimpiar;

        private DataGridView  dgvUsuarios;
        private ProgressBar   progressBar;
        private ErrorProvider errorProvider;
        private ToolTip       toolTip;

        private int _idUsuarioSeleccionado = 0;
        private readonly UsuarioServicio _servicio;

        public FrmUsuarios()
        {
            _servicio = new UsuarioServicio();
            InicializarComponentes();
            CargarPerfiles(); // carga perfiles en ComboBox antes de mostrar usuarios
            CargarUsuarios();
        }

        private void InicializarComponentes()
        {
            this.Text          = "Gestión de Usuarios";
            this.Size          = new System.Drawing.Size(900, 540);
            this.StartPosition = FormStartPosition.CenterParent;

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            toolTip       = new ToolTip();

            lblTitulo = new Label
            {
                Text     = "GESTIÓN DE USUARIOS",
                Font     = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 15),
                Size     = new System.Drawing.Size(300, 30)
            };

            // ── Nombre de usuario ────────────────────────────────────────────
            lblNombreUsuario  = new Label   { Text = "Usuario:",        Location = new System.Drawing.Point(20, 65),  Size = new System.Drawing.Size(110, 20) };
            txtNombreUsuario  = new TextBox { Location = new System.Drawing.Point(140, 63),  Size = new System.Drawing.Size(200, 25), MaxLength = 50 };
            toolTip.SetToolTip(txtNombreUsuario, "Nombre de usuario para iniciar sesión — mínimo 4 caracteres");

            // ── Contraseña ───────────────────────────────────────────────────
            lblContrasena = new Label { Text = "Contraseña:", Location = new System.Drawing.Point(20, 105), Size = new System.Drawing.Size(110, 20) };
            txtContrasena = new TextBox
            {
                Location     = new System.Drawing.Point(140, 103),
                Size         = new System.Drawing.Size(200, 25),
                PasswordChar = '*',   // oculta la contraseña
                MaxLength    = 50
            };
            toolTip.SetToolTip(txtContrasena, "Contraseña mínimo 6 caracteres. Al editar, dejar vacío para no cambiarla.");

            // ── Nombre completo ──────────────────────────────────────────────
            lblNombreCompleto = new Label   { Text = "Nombre completo:", Location = new System.Drawing.Point(20, 145), Size = new System.Drawing.Size(110, 20) };
            txtNombreCompleto = new TextBox { Location = new System.Drawing.Point(140, 143), Size = new System.Drawing.Size(200, 25), MaxLength = 100 };
            toolTip.SetToolTip(txtNombreCompleto, "Nombre completo del usuario");

            // ── Perfil (ComboBox cargado desde BD) ───────────────────────────
            lblPerfil = new Label { Text = "Perfil:", Location = new System.Drawing.Point(20, 185), Size = new System.Drawing.Size(110, 20) };
            cmbPerfil = new ComboBox
            {
                Location      = new System.Drawing.Point(140, 183),
                Size          = new System.Drawing.Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            toolTip.SetToolTip(cmbPerfil, "Perfil de acceso del usuario");

            // ── Activo ───────────────────────────────────────────────────────
            lblActivo = new Label { Text = "Activo:", Location = new System.Drawing.Point(20, 225), Size = new System.Drawing.Size(110, 20) };
            chkActivo = new CheckBox
            {
                Location = new System.Drawing.Point(140, 223),
                Checked  = true
            };

            // ── Botones ──────────────────────────────────────────────────────
            btnGuardar  = new Button { Text = "Guardar",  Location = new System.Drawing.Point(20, 265),  Size = new System.Drawing.Size(90, 35) };
            btnEditar   = new Button { Text = "Editar",   Location = new System.Drawing.Point(120, 265), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnEliminar = new Button { Text = "Eliminar", Location = new System.Drawing.Point(220, 265), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnLimpiar  = new Button { Text = "Limpiar",  Location = new System.Drawing.Point(20, 310),  Size = new System.Drawing.Size(90, 35) };

            btnGuardar.Click  += BtnGuardar_Click;
            btnEditar.Click   += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click  += (s, e) => LimpiarFormulario();

            // ── DataGridView ─────────────────────────────────────────────────
            dgvUsuarios = new DataGridView
            {
                Location            = new System.Drawing.Point(380, 60),
                Size                = new System.Drawing.Size(490, 390),
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect         = false,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };
            dgvUsuarios.SelectionChanged += DgvUsuarios_SelectionChanged;
            dgvUsuarios.DoubleClick      += DgvUsuarios_DoubleClick;

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(380, 460),
                Size     = new System.Drawing.Size(490, 20),
                Style    = ProgressBarStyle.Marquee,
                Visible  = false
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitulo,
                lblNombreUsuario,  txtNombreUsuario,
                lblContrasena,     txtContrasena,
                lblNombreCompleto, txtNombreCompleto,
                lblPerfil,         cmbPerfil,
                lblActivo,         chkActivo,
                btnGuardar, btnEditar, btnEliminar, btnLimpiar,
                dgvUsuarios, progressBar
            });
        }

        /// <summary>
        /// Carga los perfiles desde BD en el ComboBox.
        /// DisplayMember: texto visible. ValueMember: valor interno (ID).
        /// </summary>
        private void CargarPerfiles()
        {
            try
            {
                var perfiles = _servicio.ObtenerPerfiles();

                // DataSource enlaza una colección al ComboBox automáticamente
                cmbPerfil.DataSource    = perfiles;
                cmbPerfil.DisplayMember = "Nombre"; // texto que ve el usuario
                cmbPerfil.ValueMember   = "Id";     // valor que usamos internamente
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar perfiles: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                progressBar.Visible = true;
                List<Usuario> usuarios = _servicio.ObtenerTodos();

                dgvUsuarios.Rows.Clear();
                dgvUsuarios.Columns.Clear();

                dgvUsuarios.Columns.Add("IdUsuario",      "ID");
                dgvUsuarios.Columns.Add("NombreUsuario",  "Usuario");
                dgvUsuarios.Columns.Add("NombreCompleto", "Nombre Completo");
                dgvUsuarios.Columns.Add("NombrePerfil",   "Perfil");
                dgvUsuarios.Columns.Add("Activo",         "Activo");

                dgvUsuarios.Columns["IdUsuario"].Visible = false;

                foreach (Usuario u in usuarios)
                {
                    dgvUsuarios.Rows.Add(
                        u.Id,
                        u.NombreUsuario,
                        u.Nombres,
                        u.NombrePerfil,
                        u.Activo ? "Sí" : "No"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool hayError = false;

            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                errorProvider.SetError(txtNombreUsuario, "El nombre de usuario es obligatorio");
                hayError = true;
            }

            if (string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                errorProvider.SetError(txtContrasena, "La contraseña es obligatoria");
                hayError = true;
            }

            if (cmbPerfil.SelectedValue == null)
            {
                errorProvider.SetError(cmbPerfil, "Seleccione un perfil");
                hayError = true;
            }

            if (hayError) return;

            Usuario usuario = new Usuario
            {
                NombreUsuario = txtNombreUsuario.Text.Trim(),
                Nombres       = txtNombreCompleto.Text.Trim(),
                IdPerfil      = Convert.ToInt32(cmbPerfil.SelectedValue),
                Activo        = chkActivo.Checked
            };

            string error = _servicio.Insertar(usuario, txtContrasena.Text);

            if (error == null)
            {
                MessageBox.Show("Usuario guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarUsuarios();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (_idUsuarioSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un usuario para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                errorProvider.SetError(txtNombreUsuario, "El nombre de usuario es obligatorio");
                return;
            }

            Usuario usuario = new Usuario
            {
                Id            = _idUsuarioSeleccionado,
                NombreUsuario = txtNombreUsuario.Text.Trim(),
                Nombres       = txtNombreCompleto.Text.Trim(),
                IdPerfil      = Convert.ToInt32(cmbPerfil.SelectedValue),
                Activo        = chkActivo.Checked
            };

            // Si txtContrasena está vacío, el servicio no cambia la contraseña
            string error = _servicio.Actualizar(usuario, txtContrasena.Text);

            if (error == null)
            {
                MessageBox.Show("Usuario actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarUsuarios();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_idUsuarioSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un usuario para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Está seguro de que desea eliminar este usuario?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacion == DialogResult.Yes)
            {
                // Pasa el ID del usuario logueado para evitar que se elimine a sí mismo
                string error = _servicio.Eliminar(
                    _idUsuarioSeleccionado,
                    Sesion.UsuarioActual.Id
                );

                if (error == null)
                {
                    MessageBox.Show("Usuario eliminado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void DgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count > 0)
            {
                var valor = dgvUsuarios.SelectedRows[0].Cells["IdUsuario"].Value;
                _idUsuarioSeleccionado = valor != null ? Convert.ToInt32(valor) : 0;
                btnEditar.Enabled      = true;
                btnEliminar.Enabled    = true;
            }
        }

        private void DgvUsuarios_DoubleClick(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count > 0)
            {
                DataGridViewRow fila = dgvUsuarios.SelectedRows[0];

                txtNombreUsuario.Text  = fila.Cells["NombreUsuario"].Value?.ToString();
                txtNombreCompleto.Text = fila.Cells["NombreCompleto"].Value?.ToString();
                txtContrasena.Clear(); // nunca precarga la contraseña por seguridad

                // Selecciona el perfil correspondiente en el ComboBox
                string perfil = fila.Cells["NombrePerfil"].Value?.ToString();
                foreach (var item in cmbPerfil.Items)
                {
                    // Compara el nombre del perfil con cada item del ComboBox
                    if (item.ToString().Contains(perfil))
                    {
                        cmbPerfil.SelectedItem = item;
                        break;
                    }
                }

                string activo = fila.Cells["Activo"].Value?.ToString();
                chkActivo.Checked = activo == "Sí";
            }
        }

        private void LimpiarFormulario()
        {
            txtNombreUsuario.Clear();
            txtContrasena.Clear();
            txtNombreCompleto.Clear();
            chkActivo.Checked      = true;
            errorProvider.Clear();
            _idUsuarioSeleccionado = 0;
            btnEditar.Enabled      = false;
            btnEliminar.Enabled    = false;
            if (cmbPerfil.Items.Count > 0)
                cmbPerfil.SelectedIndex = 0;
            txtNombreUsuario.Focus();
        }
    }
}