using System;
using System.Collections.Generic;
using System.Windows.Forms;
using App.BLL.Servicios;
using App.DAL.Modelos;

namespace App.UI.Forms
{
    /// <summary>
    /// Formulario de gestión de Platos — módulo M1 del proyecto.
    /// Implementa CRUD completo: Crear, Leer, Actualizar, Eliminar.
    /// Todos los datos van por BLL → DAL, nunca directamente a SQL.
    /// </summary>
    public class FrmPlatos : Form
    {
        // ── Controles del formulario ─────────────────────────────────────────
        private Label          lblTitulo;
        private Label          lblNombre;
        private Label          lblCategoria;
        private Label          lblPrecio;
        private Label          lblDescripcion;
        private Label          lblDisponible;
        private Label          lblBuscar;

        private TextBox        txtNombre;
        private TextBox        txtDescripcion;
        private TextBox        txtBuscar;

        private ComboBox       cmbCategoria;   // categorías cargadas desde código
        private NumericUpDown  nudPrecio;      // control numérico para el precio
        private CheckBox       chkDisponible;  // booleano: disponible o no

        private Button         btnGuardar;
        private Button         btnEditar;
        private Button         btnEliminar;
        private Button         btnLimpiar;
        private Button         btnBuscar;

        private DataGridView   dgvPlatos;      // tabla que muestra los platos de BD
        private ProgressBar    progressBar;    // indicador visual al cargar datos

        private ErrorProvider  errorProvider;  // muestra íconos de error en campos
        private ToolTip        toolTip;        // texto de ayuda al pasar el mouse

        // ── Estado interno ───────────────────────────────────────────────────
        // Guarda el ID del plato seleccionado en el DataGridView para editar/eliminar
        private int _idPlatoSeleccionado = 0;

        // Servicio de BLL — única forma de acceder a los datos
        private readonly PlatoServicio _servicio;

        public FrmPlatos()
        {
            _servicio = new PlatoServicio();
            InicializarComponentes();
            CargarPlatos(); // carga los datos al abrir el formulario
        }

        private void InicializarComponentes()
        {
            // ── Configuración del formulario ─────────────────────────────────
            this.Text        = "Gestión de Platos";
            this.Size        = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            toolTip       = new ToolTip();

            // ── Título ───────────────────────────────────────────────────────
            lblTitulo = new Label
            {
                Text      = "GESTIÓN DE PLATOS",
                Font      = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location  = new System.Drawing.Point(20, 15),
                Size      = new System.Drawing.Size(300, 30)
            };

            // ── Panel izquierdo: formulario de captura ───────────────────────

            lblNombre = new Label { Text = "Nombre:", Location = new System.Drawing.Point(20, 60), Size = new System.Drawing.Size(100, 20) };
            txtNombre = new TextBox { Location = new System.Drawing.Point(130, 58), Size = new System.Drawing.Size(220, 25), MaxLength = 100 };
            toolTip.SetToolTip(txtNombre, "Nombre del plato — mínimo 3 caracteres");

            lblCategoria = new Label { Text = "Categoría:", Location = new System.Drawing.Point(20, 100), Size = new System.Drawing.Size(100, 20) };
            cmbCategoria = new ComboBox
            {
                Location      = new System.Drawing.Point(130, 98),
                Size          = new System.Drawing.Size(220, 25),
                DropDownStyle = ComboBoxStyle.DropDownList // solo permite seleccionar, no escribir
            };
            // Categorías fijas — en otros módulos se cargan desde BD
            cmbCategoria.Items.AddRange(new string[] { "Entrada", "Plato fuerte", "Bebida", "Postre" });
            toolTip.SetToolTip(cmbCategoria, "Seleccione la categoría del plato");

            lblPrecio = new Label { Text = "Precio ($):", Location = new System.Drawing.Point(20, 140), Size = new System.Drawing.Size(100, 20) };
            nudPrecio = new NumericUpDown
            {
                Location      = new System.Drawing.Point(130, 138),
                Size          = new System.Drawing.Size(220, 25),
                Minimum       = 0,          // valor mínimo permitido
                Maximum       = 500000,     // valor máximo permitido
                DecimalPlaces = 0,          // sin decimales
                ThousandsSeparator = true   // muestra separador de miles: 28.000
            };
            toolTip.SetToolTip(nudPrecio, "Precio del plato entre $0 y $500.000");

            lblDescripcion = new Label { Text = "Descripción:", Location = new System.Drawing.Point(20, 180), Size = new System.Drawing.Size(100, 20) };
            txtDescripcion = new TextBox
            {
                Location  = new System.Drawing.Point(130, 178),
                Size      = new System.Drawing.Size(220, 60),
                Multiline = true,     // permite múltiples líneas de texto
                MaxLength = 300,
                ScrollBars = ScrollBars.Vertical
            };
            toolTip.SetToolTip(txtDescripcion, "Descripción opcional del plato");

            lblDisponible = new Label { Text = "Disponible:", Location = new System.Drawing.Point(20, 255), Size = new System.Drawing.Size(100, 20) };
            chkDisponible = new CheckBox
            {
                Location = new System.Drawing.Point(130, 253),
                Size     = new System.Drawing.Size(20, 20),
                Checked  = true // por defecto un plato nuevo está disponible
            };
            toolTip.SetToolTip(chkDisponible, "Indica si el plato está disponible para pedir");

            // ── Botones de acción ────────────────────────────────────────────
            btnGuardar  = new Button { Text = "Guardar",  Location = new System.Drawing.Point(20, 290),  Size = new System.Drawing.Size(90, 35) };
            btnEditar   = new Button { Text = "Editar",   Location = new System.Drawing.Point(120, 290), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnEliminar = new Button { Text = "Eliminar", Location = new System.Drawing.Point(220, 290), Size = new System.Drawing.Size(90, 35), Enabled = false };
            btnLimpiar  = new Button { Text = "Limpiar",  Location = new System.Drawing.Point(20, 335),  Size = new System.Drawing.Size(90, 35) };

            btnGuardar.Click  += BtnGuardar_Click;
            btnEditar.Click   += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click  += (s, e) => LimpiarFormulario();

            // ── Panel derecho: búsqueda y DataGridView ───────────────────────
            lblBuscar  = new Label  { Text = "Buscar:", Location = new System.Drawing.Point(380, 60), Size = new System.Drawing.Size(60, 20) };
            txtBuscar  = new TextBox { Location = new System.Drawing.Point(450, 58), Size = new System.Drawing.Size(200, 25) };
            btnBuscar  = new Button  { Text = "Buscar", Location = new System.Drawing.Point(660, 56), Size = new System.Drawing.Size(80, 28) };
            btnBuscar.Click += BtnBuscar_Click;
            toolTip.SetToolTip(txtBuscar, "Buscar por nombre o categoría");

            // ── DataGridView ─────────────────────────────────────────────────
            // Control de tabla para mostrar datos de BD
            dgvPlatos = new DataGridView
            {
                Location          = new System.Drawing.Point(380, 95),
                Size              = new System.Drawing.Size(490, 390),
                ReadOnly          = true,          // no se edita directo en la tabla
                SelectionMode     = DataGridViewSelectionMode.FullRowSelect, // selecciona fila completa
                MultiSelect       = false,         // solo una fila a la vez
                AllowUserToAddRows = false,        // oculta la fila vacía de inserción
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // columnas llenan el ancho
                BackgroundColor   = System.Drawing.Color.White
            };
            // Evento: cuando el usuario hace clic en una fila del DataGridView
            dgvPlatos.SelectionChanged += DgvPlatos_SelectionChanged;
            // Evento: doble clic carga los datos en el formulario para editar
            dgvPlatos.DoubleClick     += DgvPlatos_DoubleClick;

            // ── ProgressBar ──────────────────────────────────────────────────
            // Indicador visual al cargar datos — requerimiento del proyecto
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(380, 495),
                Size     = new System.Drawing.Size(490, 20),
                Style    = ProgressBarStyle.Marquee, // animación continua (no necesita valor fijo)
                Visible  = false // oculto por defecto, se muestra al cargar
            };

            // ── Agregar controles al formulario ──────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblTitulo,
                lblNombre,    txtNombre,
                lblCategoria, cmbCategoria,
                lblPrecio,    nudPrecio,
                lblDescripcion, txtDescripcion,
                lblDisponible, chkDisponible,
                btnGuardar, btnEditar, btnEliminar, btnLimpiar,
                lblBuscar, txtBuscar, btnBuscar,
                dgvPlatos,
                progressBar
            });
        }

        /// <summary>
        /// Carga todos los platos desde BD y los muestra en el DataGridView.
        /// Muestra el ProgressBar durante la carga.
        /// </summary>
        private void CargarPlatos()
        {
            try
            {
                progressBar.Visible = true; // muestra el indicador de carga

                List<Plato> platos = _servicio.ObtenerTodos();

                // Limpia la tabla antes de cargar nuevos datos
                dgvPlatos.Rows.Clear();
                dgvPlatos.Columns.Clear();

                // Define las columnas del DataGridView manualmente
                // Esto permite controlar nombres, anchos y orden
                dgvPlatos.Columns.Add("IdPlato",     "ID");
                dgvPlatos.Columns.Add("NombrePlato", "Nombre");
                dgvPlatos.Columns.Add("Categoria",   "Categoría");
                dgvPlatos.Columns.Add("Precio",      "Precio");
                dgvPlatos.Columns.Add("Disponible",  "Disponible");

                // Oculta la columna ID — el usuario no necesita verla
                // pero la necesitamos internamente para editar/eliminar
                dgvPlatos.Columns["IdPlato"].Visible = false;

                // Recorre la lista con foreach — requerimiento del proyecto
                foreach (Plato p in platos)
                {
                    dgvPlatos.Rows.Add(
                        p.IdPlato,
                        p.NombrePlato,
                        p.Categoria,
                        p.Precio.ToString("C0"),  // formato moneda: $28.000
                        p.Disponible ? "Sí" : "No"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar platos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // finally siempre se ejecuta — oculta el progressBar pase lo que pase
                progressBar.Visible = false;
            }
        }

        /// <summary>
        /// Evento Click del botón Guardar.
        /// Valida campos, construye el objeto Plato y llama al servicio.
        /// </summary>
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Limpia errores visuales anteriores
            errorProvider.Clear();

            // ── Validaciones visuales con ErrorProvider ──────────────────────
            bool hayError = false;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errorProvider.SetError(txtNombre, "El nombre es obligatorio");
                hayError = true;
            }

            if (cmbCategoria.SelectedIndex < 0)
            {
                errorProvider.SetError(cmbCategoria, "Seleccione una categoría");
                hayError = true;
            }

            if (nudPrecio.Value <= 0)
            {
                errorProvider.SetError(nudPrecio, "El precio debe ser mayor a cero");
                hayError = true;
            }

            if (hayError) return;

            // ── Construye el objeto Plato con los datos del formulario ────────
            Plato plato = new Plato
            {
                NombrePlato = txtNombre.Text.Trim(),
                Categoria   = cmbCategoria.SelectedItem.ToString(),
                Precio      = nudPrecio.Value,
                Descripcion = txtDescripcion.Text.Trim(),
                Disponible  = chkDisponible.Checked
            };

            // ── Llama al servicio y maneja el resultado ───────────────────────
            string error = _servicio.Insertar(plato);

            if (error == null)
            {
                MessageBox.Show("Plato guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarPlatos(); // recarga el DataGridView
            }
            else
            {
                MessageBox.Show(error, "Error al guardar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Evento Click del botón Editar.
        /// Usa el ID guardado en _idPlatoSeleccionado para actualizar.
        /// </summary>
        private void BtnEditar_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            if (_idPlatoSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un plato de la tabla para editar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool hayError = false;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errorProvider.SetError(txtNombre, "El nombre es obligatorio");
                hayError = true;
            }

            if (cmbCategoria.SelectedIndex < 0)
            {
                errorProvider.SetError(cmbCategoria, "Seleccione una categoría");
                hayError = true;
            }

            if (nudPrecio.Value <= 0)
            {
                errorProvider.SetError(nudPrecio, "El precio debe ser mayor a cero");
                hayError = true;
            }

            if (hayError) return;

            Plato plato = new Plato
            {
                IdPlato     = _idPlatoSeleccionado,
                NombrePlato = txtNombre.Text.Trim(),
                Categoria   = cmbCategoria.SelectedItem.ToString(),
                Precio      = nudPrecio.Value,
                Descripcion = txtDescripcion.Text.Trim(),
                Disponible  = chkDisponible.Checked
            };

            string error = _servicio.Actualizar(plato);

            if (error == null)
            {
                MessageBox.Show("Plato actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarPlatos();
            }
            else
            {
                MessageBox.Show(error, "Error al actualizar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Evento Click del botón Eliminar.
        /// Pide confirmación antes de eliminar — requerimiento del proyecto.
        /// </summary>
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_idPlatoSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un plato de la tabla para eliminar.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmación obligatoria antes de eliminar
            var confirmacion = MessageBox.Show(
                "¿Está seguro de que desea eliminar este plato?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacion == DialogResult.Yes)
            {
                string error = _servicio.Eliminar(_idPlatoSeleccionado);

                if (error == null)
                {
                    MessageBox.Show("Plato eliminado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarPlatos();
                }
                else
                {
                    MessageBox.Show(error, "Error al eliminar",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Evento Click del botón Buscar.
        /// Filtra los platos por nombre o categoría.
        /// </summary>
        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar.Visible = true;

                List<Plato> platos = _servicio.Buscar(txtBuscar.Text);

                dgvPlatos.Rows.Clear();

                foreach (Plato p in platos)
                {
                    dgvPlatos.Rows.Add(
                        p.IdPlato,
                        p.NombrePlato,
                        p.Categoria,
                        p.Precio.ToString("C0"),
                        p.Disponible ? "Sí" : "No"
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

        /// <summary>
        /// Evento SelectionChanged del DataGridView.
        /// Cuando el usuario selecciona una fila guarda el ID del plato.
        /// Habilita los botones Editar y Eliminar.
        /// </summary>
        private void DgvPlatos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPlatos.SelectedRows.Count > 0)
            {
                // Obtiene el valor de la columna oculta IdPlato de la fila seleccionada
                var valor = dgvPlatos.SelectedRows[0].Cells["IdPlato"].Value;
                _idPlatoSeleccionado = valor != null ? Convert.ToInt32(valor) : 0;

                // Habilita editar y eliminar solo cuando hay algo seleccionado
                btnEditar.Enabled   = true;
                btnEliminar.Enabled = true;
            }
        }

        /// <summary>
        /// Evento DoubleClick del DataGridView.
        /// Carga los datos de la fila en el formulario para editar.
        /// Requerimiento: doble clic carga datos en formulario.
        /// </summary>
        private void DgvPlatos_DoubleClick(object sender, EventArgs e)
        {
            if (dgvPlatos.SelectedRows.Count > 0)
            {
                DataGridViewRow fila = dgvPlatos.SelectedRows[0];

                // Carga cada celda de la fila en el control correspondiente
                txtNombre.Text = fila.Cells["NombrePlato"].Value?.ToString();

                // Busca el índice del item en el ComboBox y lo selecciona
                string categoria = fila.Cells["Categoria"].Value?.ToString();
                int idx = cmbCategoria.Items.IndexOf(categoria);
                cmbCategoria.SelectedIndex = idx >= 0 ? idx : -1;

                // Precio viene como string con formato "$28.000" — hay que limpiar
                string precioStr = fila.Cells["Precio"].Value?.ToString()
                    .Replace("$", "").Replace(".", "").Replace(",", "").Trim();
                if (decimal.TryParse(precioStr, out decimal precio))
                    nudPrecio.Value = precio;

                string disponible = fila.Cells["Disponible"].Value?.ToString();
                chkDisponible.Checked = disponible == "Sí";
            }
        }

        /// <summary>
        /// Limpia todos los campos del formulario y resetea el estado.
        /// Se llama después de guardar, editar, eliminar o al presionar Limpiar.
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            cmbCategoria.SelectedIndex = -1; // deselecciona el ComboBox
            nudPrecio.Value            = 0;
            txtDescripcion.Clear();
            chkDisponible.Checked      = true;
            txtBuscar.Clear();
            errorProvider.Clear();

            // Resetea el estado interno y deshabilita botones de acción
            _idPlatoSeleccionado = 0;
            btnEditar.Enabled    = false;
            btnEliminar.Enabled  = false;

            txtNombre.Focus(); // pone el cursor en el primer campo
        }
    }
}