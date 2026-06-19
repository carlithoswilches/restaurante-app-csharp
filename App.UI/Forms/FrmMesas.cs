using System;
using System.Collections.Generic;
using System.Windows.Forms;
using App.BLL.Servicios;
using App.DAL.Modelos;

namespace App.UI.Forms
{
    /// <summary>
    /// Formulario de gestión de Mesas — módulo M2.
    /// Mismo patrón que FrmPlatos pero con RadioButton para ubicación
    /// y ComboBox para estado — cumple requerimiento de controles avanzados.
    /// </summary>
    public class FrmMesas : Form
    {
        // ── Controles ────────────────────────────────────────────────────────
        private Label         lblTitulo;
        private Label         lblNumero;
        private Label         lblCapacidad;
        private Label         lblEstado;
        private Label         lblUbicacion;
        private Label         lblBuscar;

        private NumericUpDown nudNumeroMesa;  // número de mesa
        private NumericUpDown nudCapacidad;   // capacidad de personas

        // ComboBox para estado — cargado desde código
        private ComboBox      cmbEstado;

        // RadioButtons para ubicación — selección única con GroupBox
        private GroupBox      grpUbicacion;
        private RadioButton   rdoInterior;
        private RadioButton   rdoTerraza;
        private RadioButton   rdoVIP;

        private TextBox       txtBuscar;
        private Button        btnGuardar;
        private Button        btnEditar;
        private Button        btnEliminar;
        private Button        btnLimpiar;
        private Button        btnBuscar;
        private DataGridView  dgvMesas;
        private ProgressBar   progressBar;
        private ErrorProvider errorProvider;
        private ToolTip       toolTip;

        private int _idMesaSeleccionada = 0;
        private readonly MesaServicio _servicio;

        public FrmMesas()
        {
            _servicio = new MesaServicio();
            InicializarComponentes();
            CargarMesas();
        }

        private void InicializarComponentes()
        {
            this.Text        = "Gestión de Mesas";
            this.Size        = new System.Drawing.Size(900, 560);
            this.StartPosition = FormStartPosition.CenterParent;

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            toolTip       = new ToolTip();

            lblTitulo = new Label
            {
                Text     = "GESTIÓN DE MESAS",
                Font     = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 15),
                Size     = new System.Drawing.Size(280, 30)
            };

            // ── Número de Mesa ───────────────────────────────────────────────
            lblNumero    = new Label       { Text = "N° Mesa:",    Location = new System.Drawing.Point(20, 65),  Size = new System.Drawing.Size(100, 20) };
            nudNumeroMesa = new NumericUpDown
            {
                Location = new System.Drawing.Point(130, 63),
                Size     = new System.Drawing.Size(100, 25),
                Minimum  = 1,
                Maximum  = 999
            };
            toolTip.SetToolTip(nudNumeroMesa, "Número identificador de la mesa");

            // ── Capacidad ────────────────────────────────────────────────────
            lblCapacidad = new Label       { Text = "Capacidad:",  Location = new System.Drawing.Point(20, 105), Size = new System.Drawing.Size(100, 20) };
            nudCapacidad  = new NumericUpDown
            {
                Location = new System.Drawing.Point(130, 103),
                Size     = new System.Drawing.Size(100, 25),
                Minimum  = 1,
                Maximum  = 20
            };
            toolTip.SetToolTip(nudCapacidad, "Número máximo de personas en la mesa");

            // ── Estado (ComboBox) ────────────────────────────────────────────
            lblEstado = new Label { Text = "Estado:", Location = new System.Drawing.Point(20, 145), Size = new System.Drawing.Size(100, 20) };
            cmbEstado = new ComboBox
            {
                Location      = new System.Drawing.Point(130, 143),
                Size          = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstado.Items.AddRange(new string[] { "Disponible", "Ocupada", "Reservada" });
            cmbEstado.SelectedIndex = 0; // por defecto: Disponible
            toolTip.SetToolTip(cmbEstado, "Estado actual de la mesa");

            // ── Ubicación (RadioButton + GroupBox) ──────────────────────────
            // GroupBox agrupa RadioButtons para que solo uno pueda estar activo
            lblUbicacion = new Label { Text = "Ubicación:", Location = new System.Drawing.Point(20, 185), Size = new System.Drawing.Size(100, 20) };
            grpUbicacion = new GroupBox
            {
                Text     = "",
                Location = new System.Drawing.Point(125, 178),
                Size     = new System.Drawing.Size(240, 35)
            };

            rdoInterior = new RadioButton { Text = "Interior", Location = new System.Drawing.Point(5,  10), Size = new Size(70,20), Checked = true };
            rdoTerraza  = new RadioButton { Text = "Terraza",  Location = new System.Drawing.Point(80, 10) ,  Size = new Size(70,20)};
            rdoVIP      = new RadioButton { Text = "VIP",      Location = new System.Drawing.Point(158, 10) ,  Size = new Size(80,20) };

            grpUbicacion.Controls.AddRange(new Control[] { rdoInterior, rdoTerraza, rdoVIP });

            // ── Botones ──────────────────────────────────────────────────────
            btnGuardar  = new Button { Text = "Guardar",  Location = new System.Drawing.Point(20, 230),  Size = new System.Drawing.Size(90, 35) };
            btnEditar   = new Button { Text = "Editar",   Location = new System.Drawing.Point(120, 230), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnEliminar = new Button { Text = "Eliminar", Location = new System.Drawing.Point(220, 230), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnLimpiar  = new Button { Text = "Limpiar",  Location = new System.Drawing.Point(20, 275),  Size = new System.Drawing.Size(90, 35) };

            btnGuardar.Click  += BtnGuardar_Click;
            btnEditar.Click   += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click  += (s, e) => LimpiarFormulario();

            // ── Búsqueda y DataGridView ──────────────────────────────────────
            lblBuscar = new Label   { Text = "Buscar:", Location = new System.Drawing.Point(380, 60), Size = new System.Drawing.Size(60, 20) };
            txtBuscar = new TextBox { Location = new System.Drawing.Point(450, 58), Size = new System.Drawing.Size(200, 25) };
            btnBuscar = new Button  { Text = "Buscar", Location = new System.Drawing.Point(660, 56), Size = new System.Drawing.Size(80, 28) };
            btnBuscar.Click += BtnBuscar_Click;

            dgvMesas = new DataGridView
            {
                Location           = new System.Drawing.Point(380, 95),
                Size               = new System.Drawing.Size(490, 360),
                ReadOnly           = true,
                SelectionMode      = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect        = false,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor    = System.Drawing.Color.White
            };
            dgvMesas.SelectionChanged += DgvMesas_SelectionChanged;
            dgvMesas.DoubleClick      += DgvMesas_DoubleClick;

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(380, 465),
                Size     = new System.Drawing.Size(490, 20),
                Style    = ProgressBarStyle.Marquee,
                Visible  = false
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitulo,
                lblNumero,    nudNumeroMesa,
                lblCapacidad, nudCapacidad,
                lblEstado,    cmbEstado,
                lblUbicacion, grpUbicacion,
                btnGuardar, btnEditar, btnEliminar, btnLimpiar,
                lblBuscar, txtBuscar, btnBuscar,
                dgvMesas, progressBar
            });
        }

        private void CargarMesas()
        {
            try
            {
                progressBar.Visible = true;
                List<Mesa> mesas = _servicio.ObtenerTodos();

                dgvMesas.Rows.Clear();
                dgvMesas.Columns.Clear();

                dgvMesas.Columns.Add("IdMesa",     "ID");
                dgvMesas.Columns.Add("NumeroMesa", "N° Mesa");
                dgvMesas.Columns.Add("Capacidad",  "Capacidad");
                dgvMesas.Columns.Add("Estado",     "Estado");
                dgvMesas.Columns.Add("Ubicacion",  "Ubicación");

                dgvMesas.Columns["IdMesa"].Visible = false;

                foreach (Mesa m in mesas)
                {
                    dgvMesas.Rows.Add(m.IdMesa, m.NumeroMesa, m.Capacidad, m.Estado, m.Ubicacion);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar mesas: {ex.Message}", "Error",
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

            if (nudNumeroMesa.Value <= 0)
            {
                errorProvider.SetError(nudNumeroMesa, "El número de mesa es obligatorio");
                hayError = true;
            }

            if (nudCapacidad.Value <= 0)
            {
                errorProvider.SetError(nudCapacidad, "La capacidad es obligatoria");
                hayError = true;
            }

            if (hayError) return;

            // Determina la ubicación según el RadioButton seleccionado
            string ubicacion = rdoInterior.Checked ? "Interior" :
                               rdoTerraza.Checked  ? "Terraza"  : "VIP";

            Mesa mesa = new Mesa
            {
                NumeroMesa = (int)nudNumeroMesa.Value,
                Capacidad  = (int)nudCapacidad.Value,
                Estado     = cmbEstado.SelectedItem.ToString(),
                Ubicacion  = ubicacion
            };

            string error = _servicio.Insertar(mesa);

            if (error == null)
            {
                MessageBox.Show("Mesa guardada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarMesas();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (_idMesaSeleccionada <= 0)
            {
                MessageBox.Show("Seleccione una mesa para editar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ubicacion = rdoInterior.Checked ? "Interior" :
                               rdoTerraza.Checked  ? "Terraza"  : "VIP";

            Mesa mesa = new Mesa
            {
                IdMesa     = _idMesaSeleccionada,
                NumeroMesa = (int)nudNumeroMesa.Value,
                Capacidad  = (int)nudCapacidad.Value,
                Estado     = cmbEstado.SelectedItem.ToString(),
                Ubicacion  = ubicacion
            };

            string error = _servicio.Actualizar(mesa);

            if (error == null)
            {
                MessageBox.Show("Mesa actualizada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarMesas();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_idMesaSeleccionada <= 0)
            {
                MessageBox.Show("Seleccione una mesa para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Está seguro de que desea eliminar esta mesa?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacion == DialogResult.Yes)
            {
                string error = _servicio.Eliminar(_idMesaSeleccionada);

                if (error == null)
                {
                    MessageBox.Show("Mesa eliminada correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarMesas();
                }
                else
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            // Para mesas filtramos localmente por estado o número
            // ya que la tabla es pequeña y no requiere consulta adicional
            CargarMesas();
        }

        private void DgvMesas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMesas.SelectedRows.Count > 0)
            {
                var valor = dgvMesas.SelectedRows[0].Cells["IdMesa"].Value;
                _idMesaSeleccionada = valor != null ? Convert.ToInt32(valor) : 0;
                btnEditar.Enabled   = true;
                btnEliminar.Enabled = true;
            }
        }

        private void DgvMesas_DoubleClick(object sender, EventArgs e)
        {
            if (dgvMesas.SelectedRows.Count > 0)
            {
                DataGridViewRow fila = dgvMesas.SelectedRows[0];

                nudNumeroMesa.Value = Convert.ToInt32(fila.Cells["NumeroMesa"].Value);
                nudCapacidad.Value  = Convert.ToInt32(fila.Cells["Capacidad"].Value);

                string estado = fila.Cells["Estado"].Value?.ToString();
                int idx = cmbEstado.Items.IndexOf(estado);
                cmbEstado.SelectedIndex = idx >= 0 ? idx : 0;

                // Selecciona el RadioButton según la ubicación guardada
                string ubicacion = fila.Cells["Ubicacion"].Value?.ToString();
                rdoInterior.Checked = ubicacion == "Interior";
                rdoTerraza.Checked  = ubicacion == "Terraza";
                rdoVIP.Checked      = ubicacion == "VIP";
            }
        }

        private void LimpiarFormulario()
        {
            nudNumeroMesa.Value     = 1;
            nudCapacidad.Value      = 1;
            cmbEstado.SelectedIndex = 0;
            rdoInterior.Checked     = true;
            txtBuscar.Clear();
            errorProvider.Clear();
            _idMesaSeleccionada = 0;
            btnEditar.Enabled   = false;
            btnEliminar.Enabled = false;
            nudNumeroMesa.Focus();
        }
    }
}