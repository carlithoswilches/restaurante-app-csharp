using System;
using System.Collections.Generic;
using System.Windows.Forms;
using App.BLL.Servicios;
using App.DAL.Modelos;

namespace App.UI.Forms
{
    /// <summary>
    /// Formulario de gestión de Clientes — módulo M4.
    /// Incluye MaskedTextBox para teléfono y documento — requerimiento del proyecto.
    /// MaskedTextBox fuerza un formato fijo de entrada al usuario.
    /// </summary>
    public class FrmClientes : Form
    {
        // ── Controles ────────────────────────────────────────────────────────
        private Label          lblTitulo;
        private Label          lblNombres;
        private Label          lblApellidos;
        private Label          lblDocumento;
        private Label          lblTelefono;
        private Label          lblEmail;
        private Label          lblActivo;
        private Label          lblBuscar;

        private TextBox        txtNombres;
        private TextBox        txtApellidos;
        private TextBox        txtEmail;

        // MaskedTextBox: fuerza formato fijo — requerimiento del proyecto
        private MaskedTextBox  mtxtDocumento;  // solo números, máx 10 dígitos
        private MaskedTextBox  mtxtTelefono;   // formato colombiano: 300 000 0000

        private CheckBox       chkActivo;      // estado activo/inactivo del cliente

        private TextBox        txtBuscar;
        private Button         btnGuardar;
        private Button         btnEditar;
        private Button         btnEliminar;
        private Button         btnLimpiar;
        private Button         btnBuscar;
        private DataGridView   dgvClientes;
        private ProgressBar    progressBar;
        private ErrorProvider  errorProvider;
        private ToolTip        toolTip;

        private int _idClienteSeleccionado = 0;
        private readonly ClienteServicio _servicio;

        public FrmClientes()
        {
            _servicio = new ClienteServicio();
            InicializarComponentes();
            CargarClientes();
        }

        private void InicializarComponentes()
        {
            this.Text          = "Gestión de Clientes";
            this.Size          = new System.Drawing.Size(950, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            toolTip       = new ToolTip();

            lblTitulo = new Label
            {
                Text     = "GESTIÓN DE CLIENTES",
                Font     = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 15),
                Size     = new System.Drawing.Size(300, 30)
            };

            // ── Nombres ──────────────────────────────────────────────────────
            lblNombres  = new Label   { Text = "Nombres:",   Location = new System.Drawing.Point(20, 65),  Size = new System.Drawing.Size(100, 20) };
            txtNombres  = new TextBox { Location = new System.Drawing.Point(130, 63),  Size = new System.Drawing.Size(200, 25), MaxLength = 80 };
            toolTip.SetToolTip(txtNombres, "Nombres del cliente");

            // ── Apellidos ────────────────────────────────────────────────────
            lblApellidos = new Label   { Text = "Apellidos:", Location = new System.Drawing.Point(20, 105), Size = new System.Drawing.Size(100, 20) };
            txtApellidos = new TextBox { Location = new System.Drawing.Point(130, 103), Size = new System.Drawing.Size(200, 25), MaxLength = 80 };
            toolTip.SetToolTip(txtApellidos, "Apellidos del cliente");

            // ── Documento (MaskedTextBox) ────────────────────────────────────
            lblDocumento  = new Label { Text = "Documento:", Location = new System.Drawing.Point(20, 145), Size = new System.Drawing.Size(100, 20) };
            mtxtDocumento = new MaskedTextBox
            {
                Location = new System.Drawing.Point(130, 143),
                Size     = new System.Drawing.Size(200, 25),
                // Máscara: 0 = dígito obligatorio, 9 = dígito opcional
                // Permite cédulas de 6 a 10 dígitos
                Mask     = "9999999999"
            };
            toolTip.SetToolTip(mtxtDocumento, "Número de documento de identidad");

            // ── Teléfono (MaskedTextBox) ─────────────────────────────────────
            lblTelefono  = new Label { Text = "Teléfono:", Location = new System.Drawing.Point(20, 185), Size = new System.Drawing.Size(100, 20) };
            mtxtTelefono = new MaskedTextBox
            {
                Location = new System.Drawing.Point(130, 183),
                Size     = new System.Drawing.Size(200, 25),
                // Formato colombiano: 300 000 0000
                Mask     = "000 000 0000"
            };
            toolTip.SetToolTip(mtxtTelefono, "Número de teléfono celular");

            // ── Email ────────────────────────────────────────────────────────
            lblEmail  = new Label   { Text = "Email:", Location = new System.Drawing.Point(20, 225), Size = new System.Drawing.Size(100, 20) };
            txtEmail  = new TextBox { Location = new System.Drawing.Point(130, 223), Size = new System.Drawing.Size(200, 25), MaxLength = 100 };
            toolTip.SetToolTip(txtEmail, "Correo electrónico — debe contener @ y punto");

            // ── Activo (CheckBox) ────────────────────────────────────────────
            lblActivo = new Label { Text = "Activo:", Location = new System.Drawing.Point(20, 265), Size = new System.Drawing.Size(100, 20) };
            chkActivo = new CheckBox
            {
                Location = new System.Drawing.Point(130, 263),
                Checked  = true
            };
            toolTip.SetToolTip(chkActivo, "Indica si el cliente está activo en el sistema");

            // ── Botones ──────────────────────────────────────────────────────
            btnGuardar  = new Button { Text = "Guardar",  Location = new System.Drawing.Point(20, 300),  Size = new System.Drawing.Size(90, 35) };
            btnEditar   = new Button { Text = "Editar",   Location = new System.Drawing.Point(120, 300), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnEliminar = new Button { Text = "Eliminar", Location = new System.Drawing.Point(220, 300), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnLimpiar  = new Button { Text = "Limpiar",  Location = new System.Drawing.Point(20, 345),  Size = new System.Drawing.Size(90, 35) };

            btnGuardar.Click  += BtnGuardar_Click;
            btnEditar.Click   += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click  += (s, e) => LimpiarFormulario();

            // ── Búsqueda y DataGridView ──────────────────────────────────────
            lblBuscar = new Label   { Text = "Buscar:", Location = new System.Drawing.Point(390, 60), Size = new System.Drawing.Size(60, 20) };
            txtBuscar = new TextBox { Location = new System.Drawing.Point(460, 58), Size = new System.Drawing.Size(200, 25) };
            btnBuscar = new Button  { Text = "Buscar", Location = new System.Drawing.Point(670, 56), Size = new System.Drawing.Size(80, 28) };
            btnBuscar.Click += BtnBuscar_Click;

            dgvClientes = new DataGridView
            {
                Location            = new System.Drawing.Point(390, 95),
                Size                = new System.Drawing.Size(530, 390),
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect         = false,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };
            dgvClientes.SelectionChanged += DgvClientes_SelectionChanged;
            dgvClientes.DoubleClick      += DgvClientes_DoubleClick;

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(390, 495),
                Size     = new System.Drawing.Size(530, 20),
                Style    = ProgressBarStyle.Marquee,
                Visible  = false
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitulo,
                lblNombres,   txtNombres,
                lblApellidos, txtApellidos,
                lblDocumento, mtxtDocumento,
                lblTelefono,  mtxtTelefono,
                lblEmail,     txtEmail,
                lblActivo,    chkActivo,
                btnGuardar, btnEditar, btnEliminar, btnLimpiar,
                lblBuscar, txtBuscar, btnBuscar,
                dgvClientes, progressBar
            });
        }

        private void CargarClientes()
        {
            try
            {
                progressBar.Visible = true;
                List<Cliente> clientes = _servicio.ObtenerTodos();

                dgvClientes.Rows.Clear();
                dgvClientes.Columns.Clear();

                dgvClientes.Columns.Add("IdCliente", "ID");
                dgvClientes.Columns.Add("Nombres",   "Nombres");
                dgvClientes.Columns.Add("Apellidos", "Apellidos");
                dgvClientes.Columns.Add("Documento", "Documento");
                dgvClientes.Columns.Add("Telefono",  "Teléfono");
                dgvClientes.Columns.Add("Email",     "Email");
                dgvClientes.Columns.Add("Activo",    "Activo");

                dgvClientes.Columns["IdCliente"].Visible = false;

                foreach (Cliente c in clientes)
                {
                    dgvClientes.Rows.Add(
                        c.Id,
                        c.Nombres,
                        c.Apellidos,
                        c.Documento,
                        c.Telefono,
                        c.Email,
                        c.Activo ? "Sí" : "No"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error",
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

            if (string.IsNullOrWhiteSpace(txtNombres.Text))
            {
                errorProvider.SetError(txtNombres, "El nombre es obligatorio");
                hayError = true;
            }

            if (string.IsNullOrWhiteSpace(txtApellidos.Text))
            {
                errorProvider.SetError(txtApellidos, "Los apellidos son obligatorios");
                hayError = true;
            }

            if (string.IsNullOrWhiteSpace(mtxtDocumento.Text.Replace(" ", "")))
            {
                errorProvider.SetError(mtxtDocumento, "El documento es obligatorio");
                hayError = true;
            }

            if (hayError) return;

            Cliente cliente = new Cliente
            {
                Nombres   = txtNombres.Text.Trim(),
                Apellidos = txtApellidos.Text.Trim(),
                // Replace elimina espacios que deja la máscara del MaskedTextBox
                Documento = mtxtDocumento.Text.Replace(" ", "").Trim(),
                Telefono  = mtxtTelefono.Text.Replace(" ", "").Trim(),
                Email     = txtEmail.Text.Trim(),
                Activo    = chkActivo.Checked
            };

            string error = _servicio.Insertar(cliente);

            if (error == null)
            {
                MessageBox.Show("Cliente guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarClientes();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (_idClienteSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un cliente para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            errorProvider.Clear();
            bool hayError = false;

            if (string.IsNullOrWhiteSpace(txtNombres.Text))
            {
                errorProvider.SetError(txtNombres, "El nombre es obligatorio");
                hayError = true;
            }

            if (string.IsNullOrWhiteSpace(txtApellidos.Text))
            {
                errorProvider.SetError(txtApellidos, "Los apellidos son obligatorios");
                hayError = true;
            }

            if (hayError) return;

            Cliente cliente = new Cliente
            {
                Id        = _idClienteSeleccionado,
                Nombres   = txtNombres.Text.Trim(),
                Apellidos = txtApellidos.Text.Trim(),
                Documento = mtxtDocumento.Text.Replace(" ", "").Trim(),
                Telefono  = mtxtTelefono.Text.Replace(" ", "").Trim(),
                Email     = txtEmail.Text.Trim(),
                Activo    = chkActivo.Checked
            };

            string error = _servicio.Actualizar(cliente);

            if (error == null)
            {
                MessageBox.Show("Cliente actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarClientes();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_idClienteSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un cliente para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Está seguro de que desea eliminar este cliente?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacion == DialogResult.Yes)
            {
                string error = _servicio.Eliminar(_idClienteSeleccionado);

                if (error == null)
                {
                    MessageBox.Show("Cliente eliminado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarClientes();
                }
                else
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar.Visible = true;
                List<Cliente> clientes = _servicio.Buscar(txtBuscar.Text);

                dgvClientes.Rows.Clear();

                foreach (Cliente c in clientes)
                {
                    dgvClientes.Rows.Add(
                        c.Id, c.Nombres, c.Apellidos,
                        c.Documento, c.Telefono, c.Email,
                        c.Activo ? "Sí" : "No"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private void DgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvClientes.SelectedRows.Count > 0)
            {
                var valor = dgvClientes.SelectedRows[0].Cells["IdCliente"].Value;
                _idClienteSeleccionado = valor != null ? Convert.ToInt32(valor) : 0;
                btnEditar.Enabled      = true;
                btnEliminar.Enabled    = true;
            }
        }

        private void DgvClientes_DoubleClick(object sender, EventArgs e)
        {
            if (dgvClientes.SelectedRows.Count > 0)
            {
                DataGridViewRow fila = dgvClientes.SelectedRows[0];

                txtNombres.Text   = fila.Cells["Nombres"].Value?.ToString();
                txtApellidos.Text = fila.Cells["Apellidos"].Value?.ToString();

                // Asigna el texto al MaskedTextBox limpiando la máscara primero
                mtxtDocumento.Text = fila.Cells["Documento"].Value?.ToString();
                mtxtTelefono.Text  = fila.Cells["Telefono"].Value?.ToString();
                txtEmail.Text      = fila.Cells["Email"].Value?.ToString();

                string activo = fila.Cells["Activo"].Value?.ToString();
                chkActivo.Checked = activo == "Sí";
            }
        }

        private void LimpiarFormulario()
        {
            txtNombres.Clear();
            txtApellidos.Clear();
            mtxtDocumento.Clear();
            mtxtTelefono.Clear();
            txtEmail.Clear();
            chkActivo.Checked      = true;
            txtBuscar.Clear();
            errorProvider.Clear();
            _idClienteSeleccionado = 0;
            btnEditar.Enabled      = false;
            btnEliminar.Enabled    = false;
            txtNombres.Focus();
        }
    }
}