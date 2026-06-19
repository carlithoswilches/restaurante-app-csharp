using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using App.BLL.Servicios;
using App.DAL.Repositorios;

namespace App.UI.Forms
{
    /// <summary>
    /// Formulario de Reportes e Importación/Exportación — módulo final.
    /// Usa TabControl para organizar los 3 reportes en pestañas separadas
    /// y una pestaña adicional para la importación de CSV.
    /// </summary>
    public class FrmReportes : Form
    {
        // TabControl: contenedor de pestañas — cada pestaña es un TabPage
        private TabControl tabControl;

        // ── Pestaña 1: Platos por Categoría (reporte sencillo) ──────────────
        private TabPage      tabPlatos;
        private DataGridView dgvPlatosCategoria;
        private Button       btnExportarPlatosExcel;
        private Button       btnExportarPlatosPDF;

        // ── Pestaña 2: Ventas por Categoría (reporte con gráfico) ───────────
        private TabPage      tabVentas;
        private DataGridView dgvVentasCategoria;
        // Panel donde dibujamos el gráfico de barras a mano con GDI+
        private Panel        panelGrafico;
        private Button       btnExportarVentasExcel;

        // ── Pestaña 3: Ventas por Rango de Fechas (reporte calculado) ──────
        private TabPage         tabFechas;
        private DateTimePicker  dtpInicio;
        private DateTimePicker  dtpFin;
        private Button          btnGenerarReporteFechas;
        private Button          btnExportarFechasPDF;
        private DataGridView    dgvVentasFechas;
        private Label           lblTotalGeneral;

        // ── Pestaña 4: Importar CSV ──────────────────────────────────────────
        private TabPage       tabImportar;
        private Button        btnSeleccionarCSV;
        private Label         lblArchivoSeleccionado;
        private DataGridView  dgvPreviewCSV;
        private Button        btnConfirmarImportacion;
        private Label         lblResultadoImportacion;

        private ProgressBar progressBar;

        // ── Servicios ────────────────────────────────────────────────────────
        private readonly ReporteServicio     _reporteServicio;
        private readonly ExportacionServicio _exportacionServicio;
        private readonly PlatoServicio       _platoServicio;

        // Guarda temporalmente los datos del CSV antes de confirmar la importación
        private List<string[]> _filasCSV = new List<string[]>();

        public FrmReportes()
        {
            _reporteServicio     = new ReporteServicio();
            _exportacionServicio = new ExportacionServicio();
            _platoServicio       = new PlatoServicio();

            InicializarComponentes();
            CargarReportePlatos();
            CargarReporteVentas();
        }

        private void InicializarComponentes()
        {
            this.Text          = "Reportes e Importación/Exportación";
            this.Size          = new System.Drawing.Size(950, 650);
            this.StartPosition = FormStartPosition.CenterParent;

            // ── TabControl principal ─────────────────────────────────────────
            tabControl = new TabControl
            {
                Location = new System.Drawing.Point(10, 10),
                Size     = new System.Drawing.Size(910, 560)
            };

            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(10, 580),
                Size     = new System.Drawing.Size(910, 20),
                Style    = ProgressBarStyle.Marquee,
                Visible  = false
            };

            ConstruirTabPlatos();
            ConstruirTabVentas();
            ConstruirTabFechas();
            ConstruirTabImportar();

            // Agrega las 4 pestañas al TabControl en orden
            tabControl.TabPages.AddRange(new TabPage[]
            {
                tabPlatos, tabVentas, tabFechas, tabImportar
            });

            this.Controls.Add(tabControl);
            this.Controls.Add(progressBar);
        }

        /// <summary>
        /// Construye la pestaña del reporte sencillo: Platos por Categoría.
        /// </summary>
        private void ConstruirTabPlatos()
        {
            tabPlatos = new TabPage("Platos por Categoría");

            var lblTitulo = new Label
            {
                Text     = "REPORTE: Platos por Categoría",
                Font     = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(15, 15),
                Size     = new System.Drawing.Size(400, 25)
            };

            dgvPlatosCategoria = new DataGridView
            {
                Location            = new System.Drawing.Point(15, 50),
                Size                = new System.Drawing.Size(860, 400),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };

            btnExportarPlatosExcel = new Button
            {
                Text     = "Exportar a Excel",
                Location = new System.Drawing.Point(15, 465),
                Size     = new System.Drawing.Size(150, 35)
            };
            btnExportarPlatosExcel.Click += BtnExportarPlatosExcel_Click;

            btnExportarPlatosPDF = new Button
            {
                Text     = "Exportar a PDF",
                Location = new System.Drawing.Point(180, 465),
                Size     = new System.Drawing.Size(150, 35)
            };
            btnExportarPlatosPDF.Click += BtnExportarPlatosPDF_Click;

            tabPlatos.Controls.AddRange(new Control[]
            {
                lblTitulo, dgvPlatosCategoria, btnExportarPlatosExcel, btnExportarPlatosPDF
            });
        }

        /// <summary>
        /// Construye la pestaña del reporte con gráfico: Ventas por Categoría.
        /// El gráfico se dibuja manualmente con el evento Paint del Panel — 
        /// esto evita depender de un control de gráficos externo.
        /// </summary>
        private void ConstruirTabVentas()
        {
            tabVentas = new TabPage("Ventas por Categoría");

            var lblTitulo = new Label
            {
                Text     = "REPORTE: Ventas por Categoría (con gráfico)",
                Font     = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(15, 15),
                Size     = new System.Drawing.Size(500, 25)
            };

            dgvVentasCategoria = new DataGridView
            {
                Location            = new System.Drawing.Point(15, 50),
                Size                = new System.Drawing.Size(420, 350),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };

            // Panel donde se dibuja el gráfico de barras
            panelGrafico = new Panel
            {
                Location        = new System.Drawing.Point(450, 50),
                Size            = new System.Drawing.Size(425, 350),
                BorderStyle     = BorderStyle.FixedSingle,
                BackColor       = System.Drawing.Color.White
            };
            // Suscribe el evento Paint — se ejecuta cada vez que el panel necesita redibujarse
            panelGrafico.Paint += PanelGrafico_Paint;

            btnExportarVentasExcel = new Button
            {
                Text     = "Exportar a Excel",
                Location = new System.Drawing.Point(15, 415),
                Size     = new System.Drawing.Size(150, 35)
            };
            btnExportarVentasExcel.Click += BtnExportarVentasExcel_Click;

            tabVentas.Controls.AddRange(new Control[]
            {
                lblTitulo, dgvVentasCategoria, panelGrafico, btnExportarVentasExcel
            });
        }

        /// <summary>
        /// Construye la pestaña del reporte calculado: Ventas por Rango de Fechas.
        /// </summary>
        private void ConstruirTabFechas()
        {
            tabFechas = new TabPage("Ventas por Fechas");

            var lblTitulo = new Label
            {
                Text     = "REPORTE: Ventas por Rango de Fechas (calculado)",
                Font     = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(15, 15),
                Size     = new System.Drawing.Size(500, 25)
            };

            var lblInicio = new Label { Text = "Desde:", Location = new System.Drawing.Point(15, 55), Size = new System.Drawing.Size(50, 20) };
            dtpInicio = new DateTimePicker
            {
                Location = new System.Drawing.Point(70, 53),
                Size     = new System.Drawing.Size(150, 25),
                // Por defecto, hace un mes — rango razonable para el reporte
                Value    = DateTime.Now.AddMonths(-1)
            };

            var lblFin = new Label { Text = "Hasta:", Location = new System.Drawing.Point(240, 55), Size = new System.Drawing.Size(50, 20) };
            dtpFin = new DateTimePicker
            {
                Location = new System.Drawing.Point(295, 53),
                Size     = new System.Drawing.Size(150, 25),
                Value    = DateTime.Now
            };

            btnGenerarReporteFechas = new Button
            {
                Text     = "Generar Reporte",
                Location = new System.Drawing.Point(460, 51),
                Size     = new System.Drawing.Size(130, 30)
            };
            btnGenerarReporteFechas.Click += BtnGenerarReporteFechas_Click;

            dgvVentasFechas = new DataGridView
            {
                Location            = new System.Drawing.Point(15, 95),
                Size                = new System.Drawing.Size(860, 350),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };

            // Label que muestra el total general calculado del rango
            lblTotalGeneral = new Label
            {
                Text      = "Total general: $0",
                Font      = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkGreen,
                Location  = new System.Drawing.Point(15, 455),
                Size      = new System.Drawing.Size(300, 25)
            };

            btnExportarFechasPDF = new Button
            {
                Text     = "Exportar a PDF",
                Location = new System.Drawing.Point(15, 490),
                Size     = new System.Drawing.Size(150, 35)
            };
            btnExportarFechasPDF.Click += BtnExportarFechasPDF_Click;

            tabFechas.Controls.AddRange(new Control[]
            {
                lblTitulo, lblInicio, dtpInicio, lblFin, dtpFin, btnGenerarReporteFechas,
                dgvVentasFechas, lblTotalGeneral, btnExportarFechasPDF
            });
        }

        /// <summary>
        /// Construye la pestaña de importación de CSV.
        /// </summary>
        private void ConstruirTabImportar()
        {
            tabImportar = new TabPage("Importar CSV");

            var lblTitulo = new Label
            {
                Text     = "IMPORTAR PLATOS DESDE CSV",
                Font     = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(15, 15),
                Size     = new System.Drawing.Size(400, 25)
            };

            var lblFormato = new Label
            {
                Text     = "Formato esperado: NombrePlato,Categoria,Precio,Descripcion",
                Location = new System.Drawing.Point(15, 50),
                Size     = new System.Drawing.Size(500, 20),
                ForeColor = System.Drawing.Color.Gray
            };

            btnSeleccionarCSV = new Button
            {
                Text     = "Seleccionar archivo CSV",
                Location = new System.Drawing.Point(15, 80),
                Size     = new System.Drawing.Size(180, 35)
            };
            btnSeleccionarCSV.Click += BtnSeleccionarCSV_Click;

            lblArchivoSeleccionado = new Label
            {
                Text     = "Ningún archivo seleccionado",
                Location = new System.Drawing.Point(210, 88),
                Size     = new System.Drawing.Size(400, 20),
                ForeColor = System.Drawing.Color.Gray
            };

            // Vista previa del CSV antes de confirmar — requerimiento del proyecto
            dgvPreviewCSV = new DataGridView
            {
                Location            = new System.Drawing.Point(15, 130),
                Size                = new System.Drawing.Size(860, 300),
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor     = System.Drawing.Color.White
            };

            btnConfirmarImportacion = new Button
            {
                Text     = "Confirmar Importación",
                Location = new System.Drawing.Point(15, 445),
                Size     = new System.Drawing.Size(180, 35),
                Enabled  = false // se habilita solo cuando hay un CSV cargado
            };
            btnConfirmarImportacion.Click += BtnConfirmarImportacion_Click;

            lblResultadoImportacion = new Label
            {
                Text     = "",
                Location = new System.Drawing.Point(15, 490),
                Size     = new System.Drawing.Size(800, 40)
            };

            tabImportar.Controls.AddRange(new Control[]
            {
                lblTitulo, lblFormato, btnSeleccionarCSV, lblArchivoSeleccionado,
                dgvPreviewCSV, btnConfirmarImportacion, lblResultadoImportacion
            });
        }

        // ════════════════════════════════════════════════════════════════════
        // LÓGICA: Reporte Platos por Categoría
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Carga el reporte sencillo de Platos por Categoría en el DataGridView.
        /// </summary>
        private void CargarReportePlatos()
        {
            try
            {
                var datos = _reporteServicio.PlatosPorCategoria();

                dgvPlatosCategoria.Rows.Clear();
                dgvPlatosCategoria.Columns.Clear();

                dgvPlatosCategoria.Columns.Add("Categoria", "Categoría");
                dgvPlatosCategoria.Columns.Add("Cantidad",  "Cantidad de Platos");

                foreach (var item in datos)
                {
                    dgvPlatosCategoria.Rows.Add(item.Categoria, item.CantidadPlatos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar reporte: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Exporta el reporte de Platos por Categoría a Excel.
        /// SaveFileDialog: ventana del sistema para elegir dónde guardar el archivo.
        /// </summary>
        private void BtnExportarPlatosExcel_Click(object sender, EventArgs e)
        {
            // SaveFileDialog es el control nativo de Windows para "Guardar como"
            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Filter   = "Archivo Excel (*.xlsx)|*.xlsx"; // solo permite guardar .xlsx
                dialogo.FileName = "PlatosPorCategoria.xlsx";       // nombre sugerido

                // ShowDialog() muestra la ventana y espera la respuesta del usuario
                // DialogResult.OK significa que el usuario eligió una ruta y dio clic en Guardar
                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        progressBar.Visible = true; // requerimiento: mostrar progreso al exportar

                        var datos = _reporteServicio.PlatosPorCategoria();
                        string error = _exportacionServicio
                            .ExportarPlatosCategoriaExcel(datos, dialogo.FileName);

                        if (error == null)
                        {
                            MessageBox.Show("Archivo Excel generado correctamente.", "Éxito",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    finally
                    {
                        progressBar.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Exporta el reporte de Platos por Categoría a PDF.
        /// </summary>
        private void BtnExportarPlatosPDF_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Filter   = "Archivo PDF (*.pdf)|*.pdf";
                dialogo.FileName = "PlatosPorCategoria.pdf";

                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        progressBar.Visible = true;

                        var datos = _reporteServicio.PlatosPorCategoria();
                        string error = _exportacionServicio
                            .ExportarPlatosCategoriaPDF(datos, dialogo.FileName);

                        if (error == null)
                        {
                            MessageBox.Show("Archivo PDF generado correctamente.", "Éxito",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    finally
                    {
                        progressBar.Visible = false;
                    }
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // LÓGICA: Reporte Ventas por Categoría + Gráfico
        // ════════════════════════════════════════════════════════════════════

        // Guarda los datos del reporte de ventas para que el evento Paint
        // del gráfico pueda acceder a ellos cuando necesite redibujar
        private List<ReporteVentasCategoria> _datosVentasCategoria = new List<ReporteVentasCategoria>();

        private void CargarReporteVentas()
        {
            try
            {
                _datosVentasCategoria = _reporteServicio.VentasPorCategoria();

                dgvVentasCategoria.Rows.Clear();
                dgvVentasCategoria.Columns.Clear();

                dgvVentasCategoria.Columns.Add("Categoria", "Categoría");
                dgvVentasCategoria.Columns.Add("Total",     "Total Ventas");
                dgvVentasCategoria.Columns.Add("Cantidad",  "Cant. Vendida");

                foreach (var item in _datosVentasCategoria)
                {
                    dgvVentasCategoria.Rows.Add(
                        item.Categoria,
                        item.TotalVentas.ToString("C0"),
                        item.CantidadVendida
                    );
                }

                // Fuerza a que el panel del gráfico se redibuje con los nuevos datos
                panelGrafico.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar reporte: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Evento Paint del panel del gráfico — se dispara automáticamente
        /// cada vez que el panel necesita pintarse (al abrir, redimensionar, etc.)
        /// Dibuja un gráfico de barras simple usando GDI+ (System.Drawing).
        /// Esto cumple el requerimiento de "reporte con gráfico" sin depender
        /// de un control de Chart externo.
        /// </summary>
        private void PanelGrafico_Paint(object sender, PaintEventArgs e)
        {
            // e.Graphics es el "lienzo" sobre el que dibujamos formas y texto
            var g = e.Graphics;

            if (_datosVentasCategoria.Count == 0)
                return; // nada que dibujar si no hay datos

            // Suaviza los bordes de las formas dibujadas
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int anchoPanel = panelGrafico.Width;
            int altoPanel  = panelGrafico.Height;
            int margenInferior = 60; // espacio para las etiquetas de categoría

            // Encuentra el valor máximo de ventas para escalar las barras proporcionalmente
            decimal maximoVenta = 0;
            foreach (var item in _datosVentasCategoria)
                if (item.TotalVentas > maximoVenta)
                    maximoVenta = item.TotalVentas;

            // Evita división por cero si todos los valores fueran 0
            if (maximoVenta == 0) maximoVenta = 1;

            // Calcula el ancho de cada barra según cuántas categorías hay
            int cantidadBarras = _datosVentasCategoria.Count;
            int anchoBarra     = (anchoPanel - 40) / cantidadBarras;

            // Colores para las barras — se repiten cíclicamente si hay más de 4 categorías
            System.Drawing.Color[] colores = {
                System.Drawing.Color.SteelBlue,
                System.Drawing.Color.Orange,
                System.Drawing.Color.MediumSeaGreen,
                System.Drawing.Color.IndianRed
            };

            int x = 20; // posición horizontal inicial
            int indice = 0;

            foreach (var item in _datosVentasCategoria)
            {
                // Calcula la altura de la barra proporcional al máximo
                // (double) es necesario porque decimal no se puede dividir directamente con int
                double proporcion = (double)(item.TotalVentas / maximoVenta);
                int alturaBarra    = (int)(proporcion * (altoPanel - margenInferior - 30));

                int y = altoPanel - margenInferior - alturaBarra;

                // Elige el color cíclicamente usando el operador módulo (%)
                var color = colores[indice % colores.Length];

                // Dibuja el rectángulo (la barra) relleno de color
                using (var brocha = new System.Drawing.SolidBrush(color))
                {
                    g.FillRectangle(brocha, x, y, anchoBarra - 10, alturaBarra);
                }

                // Dibuja el valor total encima de la barra
                using (var fuente = new System.Drawing.Font("Segoe UI", 8))
                {
                    g.DrawString(
                        item.TotalVentas.ToString("C0"),
                        fuente,
                        System.Drawing.Brushes.Black,
                        x, y - 18
                    );

                    // Dibuja el nombre de la categoría debajo de la barra
                    g.DrawString(
                        item.Categoria,
                        fuente,
                        System.Drawing.Brushes.Black,
                        x, altoPanel - margenInferior + 5
                    );
                }

                x += anchoBarra;  // avanza a la posición de la siguiente barra
                indice++;
            }
        }

        private void BtnExportarVentasExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Filter   = "Archivo Excel (*.xlsx)|*.xlsx";
                dialogo.FileName = "VentasPorCategoria.xlsx";

                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        progressBar.Visible = true;

                        string error = _exportacionServicio
                            .ExportarVentasCategoriaExcel(_datosVentasCategoria, dialogo.FileName);

                        if (error == null)
                        {
                            MessageBox.Show("Archivo Excel generado correctamente.", "Éxito",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    finally
                    {
                        progressBar.Visible = false;
                    }
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // LÓGICA: Reporte Ventas por Rango de Fechas
        // ════════════════════════════════════════════════════════════════════

        // Guarda los datos del último reporte generado para poder exportarlo después
        private List<ReporteVentasFecha> _datosVentasFechas = new List<ReporteVentasFecha>();

        private void BtnGenerarReporteFechas_Click(object sender, EventArgs e)
        {
            try
            {
                progressBar.Visible = true;

                // El servicio retorna una tupla (Datos, Error) — desempaquetamos ambos valores
                var (datos, error) = _reporteServicio
                    .VentasPorRangoFechas(dtpInicio.Value, dtpFin.Value);

                if (error != null)
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _datosVentasFechas = datos;

                dgvVentasFechas.Rows.Clear();
                dgvVentasFechas.Columns.Clear();

                dgvVentasFechas.Columns.Add("Fecha",    "Fecha");
                dgvVentasFechas.Columns.Add("Total",    "Total Ventas");
                dgvVentasFechas.Columns.Add("Cantidad", "Cantidad Pedidos");

                // Calcula el total general mientras recorre la lista — parte "calculada" del reporte
                decimal totalGeneral = 0;

                foreach (var item in datos)
                {
                    dgvVentasFechas.Rows.Add(
                        item.Fecha.ToString("dd/MM/yyyy"),
                        item.TotalVentas.ToString("C0"),
                        item.CantidadPedidos
                    );
                    totalGeneral += item.TotalVentas;
                }

                lblTotalGeneral.Text = $"Total general: {totalGeneral:C0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private void BtnExportarFechasPDF_Click(object sender, EventArgs e)
        {
            if (_datosVentasFechas.Count == 0)
            {
                MessageBox.Show("Primero genere el reporte antes de exportar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Filter   = "Archivo PDF (*.pdf)|*.pdf";
                dialogo.FileName = "VentasPorFechas.pdf";

                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        progressBar.Visible = true;

                        string error = _exportacionServicio.ExportarVentasFechaPDF(
                            _datosVentasFechas, dtpInicio.Value, dtpFin.Value, dialogo.FileName);

                        if (error == null)
                        {
                            MessageBox.Show("Archivo PDF generado correctamente.", "Éxito",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    finally
                    {
                        progressBar.Visible = false;
                    }
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // LÓGICA: Importación de CSV
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre el OpenFileDialog para seleccionar un CSV y muestra
        /// una vista previa en el DataGridView antes de confirmar.
        /// Requerimiento: vista previa antes de confirmar la importación.
        /// </summary>
        private void BtnSeleccionarCSV_Click(object sender, EventArgs e)
        {
            // OpenFileDialog: ventana nativa de Windows para elegir un archivo a abrir
            using (OpenFileDialog dialogo = new OpenFileDialog())
            {
                dialogo.Filter = "Archivos CSV (*.csv)|*.csv|Archivos de texto (*.txt)|*.txt";

                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        lblArchivoSeleccionado.Text = Path.GetFileName(dialogo.FileName);

                        // File.ReadAllLines lee el archivo completo y retorna un arreglo
                        // donde cada elemento es una línea del archivo
                        string[] lineas = File.ReadAllLines(dialogo.FileName);

                        _filasCSV.Clear();
                        dgvPreviewCSV.Rows.Clear();
                        dgvPreviewCSV.Columns.Clear();

                        dgvPreviewCSV.Columns.Add("Nombre",      "Nombre Plato");
                        dgvPreviewCSV.Columns.Add("Categoria",   "Categoría");
                        dgvPreviewCSV.Columns.Add("Precio",      "Precio");
                        dgvPreviewCSV.Columns.Add("Descripcion", "Descripción");
                        dgvPreviewCSV.Columns.Add("Estado",      "Validación");

                        int filasValidas  = 0;
                        int filasInvalidas = 0;

                        // Recorre cada línea del archivo (saltando la primera si es encabezado)
                        for (int i = 0; i < lineas.Length; i++)
                        {
                            string linea = lineas[i].Trim();

                            // Ignora líneas vacías
                            if (string.IsNullOrWhiteSpace(linea)) continue;

                            // Split divide la línea por comas — cada parte es una columna
                            string[] columnas = linea.Split(',');

                            // Si la primera línea parece encabezado (no es numérico el precio), la salta
                            if (i == 0 && columnas.Length >= 3 &&
                                !decimal.TryParse(columnas[2], out _))
                            {
                                continue; // es encabezado, no dato
                            }

                            // Valida que la fila tenga al menos 3 columnas (nombre, categoría, precio)
                            bool filaValida = columnas.Length >= 3 &&
                                !string.IsNullOrWhiteSpace(columnas[0]) &&
                                !string.IsNullOrWhiteSpace(columnas[1]) &&
                                decimal.TryParse(columnas[2], out _);

                            string descripcion = columnas.Length >= 4 ? columnas[3] : "";

                            dgvPreviewCSV.Rows.Add(
                                columnas.Length > 0 ? columnas[0] : "",
                                columnas.Length > 1 ? columnas[1] : "",
                                columnas.Length > 2 ? columnas[2] : "",
                                descripcion,
                                filaValida ? "✓ Válida" : "✗ Inválida"
                            );

                            if (filaValida)
                            {
                                _filasCSV.Add(columnas);
                                filasValidas++;
                            }
                            else
                            {
                                filasInvalidas++;
                            }
                        }

                        lblResultadoImportacion.Text =
                            $"Filas válidas: {filasValidas} | Filas inválidas: {filasInvalidas}";

                        // Solo habilita confirmar si hay al menos una fila válida
                        btnConfirmarImportacion.Enabled = filasValidas > 0;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Confirma la importación: inserta cada fila válida del CSV como un Plato nuevo.
        /// Informa cuántos registros se importaron exitosamente y cuántos fallaron.
        /// </summary>
        private void BtnConfirmarImportacion_Click(object sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show(
                $"¿Desea importar {_filasCSV.Count} platos a la base de datos?",
                "Confirmar importación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                progressBar.Visible = true;

                int exitosos = 0;
                int fallidos  = 0;

                // Recorre cada fila válida del CSV y la inserta como Plato
                foreach (string[] columnas in _filasCSV)
                {
                    var plato = new App.DAL.Modelos.Plato
                    {
                        NombrePlato = columnas[0].Trim(),
                        Categoria   = columnas[1].Trim(),
                        Precio      = decimal.Parse(columnas[2].Trim()),
                        Descripcion = columnas.Length >= 4 ? columnas[3].Trim() : "",
                        Disponible  = true
                    };

                    // Usa el mismo servicio de Platos ya construido — reutilización de código
                    string error = _platoServicio.Insertar(plato);

                    if (error == null)
                        exitosos++;
                    else
                        fallidos++;
                }

                lblResultadoImportacion.Text =
                    $"Importación completa — Exitosos: {exitosos} | Fallidos: {fallidos}";

                MessageBox.Show(
                    $"Se importaron {exitosos} platos correctamente. {fallidos} fallaron.",
                    "Importación completa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                btnConfirmarImportacion.Enabled = false;
                CargarReportePlatos(); // refresca el reporte con los nuevos platos
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la importación: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }
    }
}