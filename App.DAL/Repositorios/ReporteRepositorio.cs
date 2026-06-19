using Microsoft.Data.SqlClient;
using App.DAL.Conexion;
using System;
using System.Collections.Generic;

namespace App.DAL.Repositorios
{
    /// <summary>
    /// Clase simple para transportar datos de reportes.
    /// No es una "entidad" de BD como Plato o Cliente, solo agrupa
    /// resultados de consultas agregadas (SUM, COUNT, GROUP BY).
    /// </summary>
    public class ReportePlatosCategoria
    {
        public string Categoria        { get; set; }
        public int    CantidadPlatos   { get; set; }
    }

    public class ReporteVentasCategoria
    {
        public string  Categoria      { get; set; }
        public decimal TotalVentas    { get; set; }
        public int      CantidadVendida { get; set; }
    }

    public class ReporteVentasFecha
    {
        public DateTime Fecha       { get; set; }
        public decimal  TotalVentas { get; set; }
        public int       CantidadPedidos { get; set; }
    }

    /// <summary>
    /// Repositorio dedicado a los 3 reportes del proyecto:
    /// 1. Sencillo: Platos por categoría (solo cuenta registros)
    /// 2. Con gráfico: Ventas por categoría (suma dinero por grupo)
    /// 3. Calculado: Ventas por rango de fechas (cálculo entre dos fechas)
    /// </summary>
    public class ReporteRepositorio
    {
        /// <summary>
        /// REPORTE SENCILLO: cuenta cuántos platos hay por cada categoría.
        /// GROUP BY agrupa las filas que comparten el mismo valor de Categoria,
        /// y COUNT(*) cuenta cuántas filas cayeron en cada grupo.
        /// </summary>
        public List<ReportePlatosCategoria> PlatosPorCategoria()
        {
            var lista = new List<ReportePlatosCategoria>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                // GROUP BY Categoria: une todas las filas con la misma categoría
                // COUNT(*): cuenta cuántas filas hay en cada grupo
                string sql = @"SELECT Categoria, COUNT(*) AS CantidadPlatos
                               FROM Platos
                               GROUP BY Categoria
                               ORDER BY Categoria";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new ReportePlatosCategoria
                                {
                                    Categoria      = reader.GetString(0),
                                    CantidadPlatos = reader.GetInt32(1)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error en reporte de platos: {ex.Message}");
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// REPORTE CON GRÁFICO: suma el dinero vendido por categoría.
        /// Hace JOIN entre 3 tablas: DetallePedido -> Platos (para saber la categoría)
        /// y solo cuenta pedidos que NO estén cancelados.
        /// </summary>
        public List<ReporteVentasCategoria> VentasPorCategoria()
        {
            var lista = new List<ReporteVentasCategoria>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                // INNER JOIN: conecta DetallePedido con Platos por IdPlato
                // y con Pedidos por IdPedido, para poder filtrar por Estado
                // SUM(d.Subtotal): suma todos los subtotales de ese grupo
                string sql = @"SELECT pl.Categoria, 
                                      SUM(d.Subtotal)  AS TotalVentas,
                                      SUM(d.Cantidad)  AS CantidadVendida
                               FROM DetallePedido d
                               INNER JOIN Platos  pl ON d.IdPlato  = pl.IdPlato
                               INNER JOIN Pedidos p  ON d.IdPedido = p.IdPedido
                               WHERE p.Estado <> 'Cancelado'
                               GROUP BY pl.Categoria
                               ORDER BY TotalVentas DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new ReporteVentasCategoria
                                {
                                    Categoria       = reader.GetString(0),
                                    TotalVentas     = reader.GetDecimal(1),
                                    CantidadVendida = reader.GetInt32(2)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error en reporte de ventas: {ex.Message}");
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// REPORTE CALCULADO: ventas totales agrupadas por día, 
        /// filtrando solo entre dos fechas que el usuario elige.
        /// CAST(p.FechaPedido AS DATE) quita la hora, deja solo la fecha,
        /// para poder agrupar pedidos del mismo día aunque tengan horas distintas.
        /// </summary>
        public List<ReporteVentasFecha> VentasPorRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<ReporteVentasFecha>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"SELECT CAST(p.FechaPedido AS DATE) AS Fecha,
                                      SUM(p.Total)   AS TotalVentas,
                                      COUNT(*)       AS CantidadPedidos
                               FROM Pedidos p
                               WHERE p.FechaPedido BETWEEN @FechaInicio AND @FechaFin
                                 AND p.Estado <> 'Cancelado'
                               GROUP BY CAST(p.FechaPedido AS DATE)
                               ORDER BY Fecha";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    // BETWEEN necesita ambos límites — agregamos un día a fechaFin
                    // para incluir todo ese día completo (hasta las 23:59:59)
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmd.Parameters.AddWithValue("@FechaFin",    fechaFin.Date.AddDays(1).AddSeconds(-1));

                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new ReporteVentasFecha
                                {
                                    Fecha           = reader.GetDateTime(0),
                                    TotalVentas     = reader.GetDecimal(1),
                                    CantidadPedidos = reader.GetInt32(2)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error en reporte por fechas: {ex.Message}");
                    }
                }
            }
            return lista;
        }
    }
}