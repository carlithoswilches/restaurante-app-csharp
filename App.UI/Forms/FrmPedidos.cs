using System;
using System.Collections.Generic;
using System.Windows.Forms;
using App.BLL.Servicios;
using App.DAL.Modelos;

namespace App.UI.Forms
{
    /// <summary>
    /// Formulario de gestión de Pedidos — módulo M3.
    /// Es el más complejo: combina Mesa, Cliente y varios Platos
    /// en una sola operación con transacción SQL.
    /// Usa Queue<DetallePedido> para la cola de platos — requerimiento de colecciones.
    /// </summary>
    public class FrmPedidos : Form
    {
        // ── Controles cabecera ───────────────────────────────────────────────
        private Label        lblTitulo;
        private Label        lblMesa;
        private Label        lblCliente;
        private Label        lblEstado;
        private Label        lblFecha;

        private ComboBox     cmbMesa;      // mesas cargadas desde BD
        private ComboBox     cmbCliente;   // clientes cargados desde BD
        private ComboBox     cmbEstado;    // estados del pedido
        private DateTimePicker dtpFecha;  // fecha del pedido — requerimiento

        // ── Controles detalle ────────────────────────────────────────────────
        private Label        lblPlato;
        private Label        lblCantidad;
        private Label        lblTotalPedido;

        private ComboBox     cmbPlato;     // platos disponibles desde BD
        private NumericUpDown nudCantidad; // cantidad del plato
        private Button       btnAgregarPlato;
        private Button       btnQuitarPlato;
        private ListBox      lstDetalle;   // muestra los platos agregados al pedido
        private Label        lblTotal;     // muestra el total calculado

        // ── Controles pedidos existentes ─────────────────────────────────────
        private DataGridView dgvPedidos;
        private DataGridView dgvDetallePedido; // detalle del pedido seleccionado

        // ── Botones principales ──────────────────────────────────────────────
        private Button       btnGuardar;
        private Button       btnEliminar;
        private Button       btnActualizarEstado;
        private Button       btnLimpiar;

        private ProgressBar  progressBar;
        private ErrorProvider errorProvider;
        private ToolTip      toolTip;

        // ── Estado interno ───────────────────────────────────────────────────
        private int _idPedidoSeleccionado = 0;

        // Queue<T>: cola FIFO — el primer plato agregado es el primero en mostrarse
        // Requerimiento explícito del proyecto: usar Queue o Stack
        private Queue<DetallePedido> _colaPedido;

        // Servicios de las 3 capas que necesita este formulario
        private readonly PedidoServicio  _pedidoServicio;
        private readonly PlatoServicio   _platoServicio;
        private readonly MesaServicio    _mesaServicio;
        private readonly ClienteServicio _clienteServicio;

        public FrmPedidos()
        {
            _pedidoServicio  = new PedidoServicio();
            _platoServicio   = new PlatoServicio();
            _mesaServicio    = new MesaServicio();
            _clienteServicio = new ClienteServicio();
            _colaPedido      = new Queue<DetallePedido>();

            InicializarComponentes();
            CargarCombos();
            CargarPedidos();
        }

        private void InicializarComponentes()
        {
            this.Text          = "Gestión de Pedidos";
            this.Size          = new System.Drawing.Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterParent;

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            toolTip       = new ToolTip();

            lblTitulo = new Label
            {
                Text     = "GESTIÓN DE PEDIDOS",
                Font     = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 15),
                Size     = new System.Drawing.Size(300, 30)
            };

            // ── Cabecera del pedido ──────────────────────────────────────────
            lblMesa    = new Label { Text = "Mesa:",    Location = new System.Drawing.Point(20, 60),  Size = new System.Drawing.Size(80, 20) };
            cmbMesa    = new ComboBox
            {
                Location      = new System.Drawing.Point(110, 58),
                Size          = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            toolTip.SetToolTip(cmbMesa, "Seleccione la mesa del pedido");

            lblCliente = new Label { Text = "Cliente:", Location = new System.Drawing.Point(20, 100), Size = new System.Drawing.Size(80, 20) };
            cmbCliente = new ComboBox
            {
                Location      = new System.Drawing.Point(110, 98),
                Size          = new System.Drawing.Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            toolTip.SetToolTip(cmbCliente, "Seleccione el cliente");

            lblFecha = new Label { Text = "Fecha:", Location = new System.Drawing.Point(20, 140), Size = new System.Drawing.Size(80, 20) };
            dtpFecha = new DateTimePicker
            {
                Location = new System.Drawing.Point(110, 138),
                Size     = new System.Drawing.Size(200, 25),
                Value    = DateTime.Now,
                Format   = DateTimePickerFormat.Short
            };
            toolTip.SetToolTip(dtpFecha, "Fecha del pedido");

            lblEstado = new Label { Text = "Estado:", Location = new System.Drawing.Point(20, 180), Size = new System.Drawing.Size(80, 20) };
            cmbEstado = new ComboBox
            {
                Location      = new System.Drawing.Point(110, 178),
                Size          = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstado.Items.AddRange(new string[]
                { "Pendiente", "En preparación", "Entregado", "Cancelado" });
            cmbEstado.SelectedIndex = 0;

            // ── Sección de detalle — agregar platos ──────────────────────────
            lblPlato    = new Label { Text = "Plato:",    Location = new System.Drawing.Point(20, 225), Size = new System.Drawing.Size(80, 20) };
            cmbPlato    = new ComboBox
            {
                Location      = new System.Drawing.Point(110, 223),
                Size          = new System.Drawing.Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            lblCantidad = new Label { Text = "Cantidad:", Location = new System.Drawing.Point(20, 265), Size = new System.Drawing.Size(80, 20) };
            nudCantidad = new NumericUpDown
            {
                Location = new System.Drawing.Point(110, 263),
                Size     = new System.Drawing.Size(80, 25),
                Minimum  = 1,
                Maximum  = 50,
                Value    = 1
            };

            btnAgregarPlato = new Button
            {
                Text     = "+ Agregar",
                Location = new System.Drawing.Point(110, 300),
                Size     = new System.Drawing.Size(100, 30)
            };
            btnAgregarPlato.Click += BtnAgregarPlato_Click;

            btnQuitarPlato = new Button
            {
                Text     = "- Quitar",
                Location = new System.Drawing.Point(220, 300),
                Size     = new System.Drawing.Size(90, 30)
            };
            btnQuitarPlato.Click += BtnQuitarPlato_Click;

            // ListBox: muestra los platos en la cola del pedido actual
            lstDetalle = new ListBox
            {
                Location = new System.Drawing.Point(20, 340),
                Size     = new System.Drawing.Size(300, 150)
            };

            // Label que muestra el total calculado en tiempo real
            lblTotalPedido = new Label { Text = "Total:", Location = new System.Drawing.Point(20, 500), Size = new System.Drawing.Size(60, 20) };
            lblTotal = new Label
            {
                Text      = "$0",
                Location  = new System.Drawing.Point(85, 500),
                Size      = new System.Drawing.Size(120, 20),
                Font      = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkGreen
            };

            // ── Botones principales ──────────────────────────────────────────
            btnGuardar = new Button
            {
                Text     = "Guardar Pedido",
                Location = new System.Drawing.Point(20, 530),
                Size     = new System.Drawing.Size(130, 35)
            };
            btnGuardar.Click += BtnGuardar_Click;

            btnActualizarEstado = new Button
            {
                Text     = "Actualizar Estado",
                Location = new System.Drawing.Point(160, 530),
                Size     = new System.Drawing.Size(140, 35),
                Enabled  = false
            };
            btnActualizarEstado.Click += BtnActualizarEstado_Click;

            btnEliminar = new Button
            {
                Text     = "Eliminar",
                Location = new System.Drawing.Point(20, 575),
                Size     = new System.Drawing.Size(100, 35),
                Enabled  = false
            };
            btnEliminar.Click += BtnEliminar_Click;

            btnLimpiar = new Button
            {
                Text     = "Limpiar",
                Location = new System.Drawing.Point(130, 575),
                Size     = new System.Drawing.Size(100, 35)
            };
            btnLimpiar.Click += (s, e) => LimpiarFormulario();

            // ── Panel derecho: pedidos existentes ────────────────────────────
            var lblPedidos = new Label
            {
                Text     = "Pedidos registrados:",
                Location = new System.Drawing.Point(360, 60),
                Size     = new System.Drawing.Size(200, 20),
                Font     = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };

            dgvPedidos = new DataGridView
            {
                Location            = new System.Drawing.Point(360, 85),
                Size                = new System.Drawing.Size(700, 250),
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect         = false,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };
            dgvPedidos.SelectionChanged += DgvPedidos_SelectionChanged;

            var lblDetallePedido = new Label
            {
                Text     = "Detalle del pedido seleccionado:",
                Location = new System.Drawing.Point(360, 345),
                Size     = new System.Drawing.Size(280, 20),
                Font     = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };

            // DataGridView secundario — muestra el detalle del pedido seleccionado
            dgvDetallePedido = new DataGridView
            {
                Location            = new System.Drawing.Point(360, 370),
                Size                = new System.Drawing.Size(700, 200),
                ReadOnly            = true,
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(360, 580),
                Size     = new System.Drawing.Size(700, 20),
                Style    = ProgressBarStyle.Marquee,
                Visible  = false
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitulo,
                lblMesa,    cmbMesa,
                lblCliente, cmbCliente,
                lblFecha,   dtpFecha,
                lblEstado,  cmbEstado,
                lblPlato,   cmbPlato,
                lblCantidad, nudCantidad,
                btnAgregarPlato, btnQuitarPlato,
                lstDetalle,
                lblTotalPedido, lblTotal,
                btnGuardar, btnActualizarEstado,
                btnEliminar, btnLimpiar,
                lblPedidos, dgvPedidos,
                lblDetallePedido, dgvDetallePedido,
                progressBar
            });
        }

        /// <summary>
        /// Carga los ComboBox de Mesa, Cliente y Plato desde BD.
        /// </summary>
        private void CargarCombos()
        {
            try
            {
                // ── Mesas ────────────────────────────────────────────────────
                List<Mesa> mesas = _mesaServicio.ObtenerTodos();
                cmbMesa.DataSource    = mesas;
                cmbMesa.DisplayMember = "NumeroMesa";
                cmbMesa.ValueMember   = "IdMesa";

                // ── Clientes ─────────────────────────────────────────────────
                List<Cliente> clientes = _clienteServicio.ObtenerTodos();
                cmbCliente.DataSource    = clientes;
                cmbCliente.DisplayMember = "Nombres";
                cmbCliente.ValueMember   = "Id";

                // ── Platos disponibles ───────────────────────────────────────
                List<Plato> platos = _platoServicio.ObtenerTodos();
                cmbPlato.DataSource    = platos;
                cmbPlato.DisplayMember = "NombrePlato";
                cmbPlato.ValueMember   = "IdPlato";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarPedidos()
        {
            try
            {
                progressBar.Visible = true;
                List<Pedido> pedidos = _pedidoServicio.ObtenerTodos();

                dgvPedidos.Rows.Clear();
                dgvPedidos.Columns.Clear();

                dgvPedidos.Columns.Add("IdPedido",      "ID");
                dgvPedidos.Columns.Add("NombreMesa",    "Mesa");
                dgvPedidos.Columns.Add("NombreCliente", "Cliente");
                dgvPedidos.Columns.Add("FechaPedido",   "Fecha");
                dgvPedidos.Columns.Add("Estado",        "Estado");
                dgvPedidos.Columns.Add("Total",         "Total");

                dgvPedidos.Columns["IdPedido"].Visible = false;

                foreach (Pedido p in pedidos)
                {
                    dgvPedidos.Rows.Add(
                        p.IdPedido,
                        p.NombreMesa,
                        p.NombreCliente,
                        p.FechaPedido.ToString("dd/MM/yyyy HH:mm"),
                        p.Estado,
                        p.Total.ToString("C0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar pedidos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        /// <summary>
        /// Agrega un plato a la Queue del pedido actual.
        /// Queue.Enqueue: agrega al final de la cola.
        /// </summary>
        private void BtnAgregarPlato_Click(object sender, EventArgs e)
        {
            if (cmbPlato.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un plato.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obtiene el plato seleccionado del DataSource del ComboBox
            Plato platoSeleccionado = (Plato)cmbPlato.SelectedItem;

            DetallePedido detalle = new DetallePedido
            {
                IdPlato        = platoSeleccionado.IdPlato,
                NombrePlato    = platoSeleccionado.NombrePlato,
                Cantidad       = (int)nudCantidad.Value,
                PrecioUnitario = platoSeleccionado.Precio,
                Subtotal       = platoSeleccionado.Precio * (int)nudCantidad.Value
            };

            // Enqueue: agrega el detalle al final de la cola FIFO
            _colaPedido.Enqueue(detalle);

            ActualizarListaYTotal();
        }

        /// <summary>
        /// Quita el último plato agregado usando Dequeue.
        /// Queue.Dequeue: saca el primer elemento (FIFO — el más antiguo).
        /// </summary>
        private void BtnQuitarPlato_Click(object sender, EventArgs e)
        {
            if (_colaPedido.Count == 0)
            {
                MessageBox.Show("No hay platos para quitar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Para quitar el último usamos una cola auxiliar
            // Transferimos todos excepto el último y reconstruimos
            var temporal = new Queue<DetallePedido>();
            var lista    = new List<DetallePedido>(_colaPedido);

            // Quita el último elemento de la lista
            lista.RemoveAt(lista.Count - 1);

            // Reconstruye la cola con los elementos restantes
            _colaPedido.Clear();
            foreach (var item in lista)
                _colaPedido.Enqueue(item);

            ActualizarListaYTotal();
        }

        /// <summary>
        /// Actualiza el ListBox y el label de total con los platos de la cola.
        /// foreach sobre Queue recorre en orden FIFO sin sacar elementos.
        /// </summary>
        private void ActualizarListaYTotal()
        {
            lstDetalle.Items.Clear();
            decimal total = 0;

            // foreach sobre Queue: recorre sin modificar la cola
            foreach (DetallePedido d in _colaPedido)
            {
                lstDetalle.Items.Add(
                    $"{d.NombrePlato} x{d.Cantidad} = {d.Subtotal:C0}"
                );
                total += d.Subtotal;
            }

            lblTotal.Text = total.ToString("C0");
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool hayError = false;

            if (cmbMesa.SelectedValue == null)
            {
                errorProvider.SetError(cmbMesa, "Seleccione una mesa");
                hayError = true;
            }

            if (cmbCliente.SelectedValue == null)
            {
                errorProvider.SetError(cmbCliente, "Seleccione un cliente");
                hayError = true;
            }

            if (_colaPedido.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un plato al pedido.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                hayError = true;
            }

            if (hayError) return;

            Pedido pedido = new Pedido
            {
                IdMesa    = Convert.ToInt32(cmbMesa.SelectedValue),
                IdCliente = Convert.ToInt32(cmbCliente.SelectedValue),
                // IdUsuario: el usuario logueado toma el pedido
                IdUsuario   = Sesion.UsuarioActual.Id,
                FechaPedido = dtpFecha.Value,
                Estado      = cmbEstado.SelectedItem.ToString(),
                // Convierte la Queue a List para enviarlo al servicio
                Detalle     = new List<DetallePedido>(_colaPedido)
            };

            string error = _pedidoServicio.Insertar(pedido);

            if (error == null)
            {
                MessageBox.Show("Pedido guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarFormulario();
                CargarPedidos();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Actualiza solo el estado del pedido seleccionado.
        /// </summary>
        private void BtnActualizarEstado_Click(object sender, EventArgs e)
        {
            if (_idPedidoSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un pedido.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string error = _pedidoServicio.ActualizarEstado(
                _idPedidoSeleccionado,
                cmbEstado.SelectedItem.ToString()
            );

            if (error == null)
            {
                MessageBox.Show("Estado actualizado.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarPedidos();
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_idPedidoSeleccionado <= 0)
            {
                MessageBox.Show("Seleccione un pedido para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Está seguro de que desea eliminar este pedido?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacion == DialogResult.Yes)
            {
                string error = _pedidoServicio.Eliminar(_idPedidoSeleccionado);

                if (error == null)
                {
                    MessageBox.Show("Pedido eliminado.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarPedidos();
                }
                else
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Al seleccionar un pedido en el DataGridView carga su detalle.
        /// </summary>
        private void DgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPedidos.SelectedRows.Count > 0)
            {
                var valor = dgvPedidos.SelectedRows[0].Cells["IdPedido"].Value;
                _idPedidoSeleccionado = valor != null ? Convert.ToInt32(valor) : 0;

                btnEliminar.Enabled         = true;
                btnActualizarEstado.Enabled = true;

                // Carga el detalle del pedido seleccionado
                CargarDetallePedido(_idPedidoSeleccionado);
            }
        }

        private void CargarDetallePedido(int idPedido)
        {
            try
            {
                List<DetallePedido> detalle = _pedidoServicio.ObtenerDetalle(idPedido);

                dgvDetallePedido.Rows.Clear();
                dgvDetallePedido.Columns.Clear();

                dgvDetallePedido.Columns.Add("NombrePlato",    "Plato");
                dgvDetallePedido.Columns.Add("Cantidad",       "Cantidad");
                dgvDetallePedido.Columns.Add("PrecioUnitario", "Precio Unit.");
                dgvDetallePedido.Columns.Add("Subtotal",       "Subtotal");

                foreach (DetallePedido d in detalle)
                {
                    dgvDetallePedido.Rows.Add(
                        d.NombrePlato,
                        d.Cantidad,
                        d.PrecioUnitario.ToString("C0"),
                        d.Subtotal.ToString("C0")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalle: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormulario()
        {
            // Limpia la cola de platos del pedido actual
            _colaPedido.Clear();
            lstDetalle.Items.Clear();
            lblTotal.Text           = "$0";
            dtpFecha.Value          = DateTime.Now;
            cmbEstado.SelectedIndex = 0;
            nudCantidad.Value       = 1;
            errorProvider.Clear();
            _idPedidoSeleccionado       = 0;
            btnEliminar.Enabled         = false;
            btnActualizarEstado.Enabled = false;

            if (cmbMesa.Items.Count    > 0) cmbMesa.SelectedIndex    = 0;
            if (cmbCliente.Items.Count > 0) cmbCliente.SelectedIndex = 0;
            if (cmbPlato.Items.Count   > 0) cmbPlato.SelectedIndex   = 0;
        }
    }
}