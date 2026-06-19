using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using App.DAL.Repositorios;
using System;
using System.Collections.Generic;

namespace App.BLL.Servicios
{
    /// <summary>
    /// Servicio responsable de exportar datos a Excel y PDF.
    /// Recibe listas genéricas de datos y las convierte en archivos físicos.
    /// </summary>
    public class ExportacionServicio
    {
        /// <summary>
        /// Exporta el reporte de Platos por Categoría a un archivo Excel (.xlsx).
        /// ClosedXML crea el archivo en memoria y lo guarda en la ruta indicada.
        /// </summary>
        public string ExportarPlatosCategoriaExcel(
            List<ReportePlatosCategoria> datos, string rutaArchivo)
        {
            try
            {
                // XLWorkbook representa el archivo Excel completo (como libro.xlsx)
                using (var workbook = new XLWorkbook())
                {
                    // Worksheet = una pestaña/hoja dentro del archivo Excel
                    var hoja = workbook.Worksheets.Add("Platos por Categoría");

                    // ── Encabezados de columnas (fila 1) ──────────────────────
                    hoja.Cell(1, 1).Value = "Categoría";
                    hoja.Cell(1, 2).Value = "Cantidad de Platos";

                    // Pone en negrita la fila de encabezados
                    hoja.Row(1).Style.Font.Bold = true;

                    // ── Llena los datos fila por fila empezando en la fila 2 ──
                    int fila = 2;
                    foreach (var item in datos)
                    {
                        hoja.Cell(fila, 1).Value = item.Categoria;
                        hoja.Cell(fila, 2).Value = item.CantidadPlatos;
                        fila++; // avanza a la siguiente fila
                    }

                    // Ajusta automáticamente el ancho de las columnas al contenido
                    hoja.Columns().AdjustToContents();

                    // Guarda el archivo físicamente en la ruta indicada
                    workbook.SaveAs(rutaArchivo);
                }

                return null; // null significa éxito
            }
            catch (Exception ex)
            {
                return $"Error al exportar a Excel: {ex.Message}";
            }
        }

        /// <summary>
        /// Exporta el reporte de Ventas por Categoría a un archivo Excel.
        /// Mismo patrón que el anterior pero con más columnas.
        /// </summary>
        public string ExportarVentasCategoriaExcel(
            List<ReporteVentasCategoria> datos, string rutaArchivo)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var hoja = workbook.Worksheets.Add("Ventas por Categoría");

                    hoja.Cell(1, 1).Value = "Categoría";
                    hoja.Cell(1, 2).Value = "Total Ventas";
                    hoja.Cell(1, 3).Value = "Cantidad Vendida";
                    hoja.Row(1).Style.Font.Bold = true;

                    int fila = 2;
                    foreach (var item in datos)
                    {
                        hoja.Cell(fila, 1).Value = item.Categoria;
                        hoja.Cell(fila, 2).Value = item.TotalVentas;
                        hoja.Cell(fila, 3).Value = item.CantidadVendida;
                        fila++;
                    }

                    hoja.Columns().AdjustToContents();
                    workbook.SaveAs(rutaArchivo);
                }
                return null;
            }
            catch (Exception ex)
            {
                return $"Error al exportar a Excel: {ex.Message}";
            }
        }

        /// <summary>
        /// Genera un PDF del reporte de Platos por Categoría usando QuestPDF.
        /// QuestPDF construye el documento declarando su estructura visual en código,
        /// similar a como se construye una página con HTML pero en C#.
        /// </summary>
        public string ExportarPlatosCategoriaPDF(
            List<ReportePlatosCategoria> datos, string rutaArchivo)
        {
            try
            {
                // Licencia gratuita de QuestPDF para uso no comercial/educativo
                QuestPDF.Settings.License = LicenseType.Community;

                // Document.Create recibe un contenedor donde se define el contenido
                Document.Create(documento =>
                {
                    // Page = una página del PDF (puede haber varias)
                    documento.Page(pagina =>
                    {
                        pagina.Size(PageSizes.A4);   // tamaño de hoja carta A4
                        pagina.Margin(40);            // margen en todos los bordes

                        // Encabezado del documento
                        pagina.Header()
                            .Text("Reporte de Platos por Categoría")
                            .FontSize(18)
                            .Bold();

                        // Contenido principal — aquí va la tabla
                        pagina.Content().Table(tabla =>
                        {
                            // Define las columnas de la tabla — 2 columnas iguales
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.RelativeColumn();
                                columnas.RelativeColumn();
                            });

                            // Encabezado de la tabla
                            tabla.Header(encabezado =>
                            {
                                encabezado.Cell().Text("Categoría").Bold();
                                encabezado.Cell().Text("Cantidad de Platos").Bold();
                            });

                            // Una fila por cada categoría del reporte
                            foreach (var item in datos)
                            {
                                tabla.Cell().Text(item.Categoria);
                                tabla.Cell().Text(item.CantidadPlatos.ToString());
                            }
                        });

                        // Pie de página con fecha de generación
                        pagina.Footer()
                            .AlignCenter()
                            .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
                })
                // GeneratePdf guarda el documento construido como archivo físico
                .GeneratePdf(rutaArchivo);

                return null;
            }
            catch (Exception ex)
            {
                return $"Error al exportar a PDF: {ex.Message}";
            }
        }

        /// <summary>
        /// Genera un PDF del reporte de Ventas por Rango de Fechas.
        /// Incluye un total general al final — ejemplo de reporte "calculado".
        /// </summary>
        public string ExportarVentasFechaPDF(
            List<ReporteVentasFecha> datos, DateTime inicio, DateTime fin, string rutaArchivo)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                // Calcula el total general sumando todos los TotalVentas de la lista
                decimal totalGeneral = 0;
                foreach (var item in datos)
                    totalGeneral += item.TotalVentas;

                Document.Create(documento =>
                {
                    documento.Page(pagina =>
                    {
                        pagina.Size(PageSizes.A4);
                        pagina.Margin(40);

                        pagina.Header().Column(columna =>
                        {
                            columna.Item().Text("Reporte de Ventas por Rango de Fechas")
                                .FontSize(18).Bold();
                            columna.Item().Text($"Del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}")
                                .FontSize(11);
                        });

                        pagina.Content().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.RelativeColumn();
                                columnas.RelativeColumn();
                                columnas.RelativeColumn();
                            });

                            tabla.Header(encabezado =>
                            {
                                encabezado.Cell().Text("Fecha").Bold();
                                encabezado.Cell().Text("Total Ventas").Bold();
                                encabezado.Cell().Text("Cantidad Pedidos").Bold();
                            });

                            foreach (var item in datos)
                            {
                                tabla.Cell().Text(item.Fecha.ToString("dd/MM/yyyy"));
                                tabla.Cell().Text(item.TotalVentas.ToString("C0"));
                                tabla.Cell().Text(item.CantidadPedidos.ToString());
                            }
                        });

                        // Total general al final del documento — parte "calculada" del reporte
                        pagina.Footer().Column(columna =>
                        {
                            columna.Item().AlignRight()
                                .Text($"TOTAL GENERAL: {totalGeneral:C0}")
                                .Bold().FontSize(13);
                            columna.Item().AlignCenter()
                                .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(8);
                        });
                    });
                })
                .GeneratePdf(rutaArchivo);

                return null;
            }
            catch (Exception ex)
            {
                return $"Error al exportar a PDF: {ex.Message}";
            }
        }
    }
}