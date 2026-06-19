using App.DAL.Repositorios;
using System;
using System.Collections.Generic;

namespace App.BLL.Servicios
{
    /// <summary>
    /// Servicio de Reportes: valida parámetros (como el rango de fechas)
    /// antes de consultar la BD. Los reportes en sí no necesitan reglas
    /// de negocio complejas porque son solo consultas de lectura.
    /// </summary>
    public class ReporteServicio
    {
        private readonly ReporteRepositorio _repositorio;

        public ReporteServicio()
        {
            _repositorio = new ReporteRepositorio();
        }

        public List<ReportePlatosCategoria> PlatosPorCategoria()
        {
            return _repositorio.PlatosPorCategoria();
        }

        public List<ReporteVentasCategoria> VentasPorCategoria()
        {
            return _repositorio.VentasPorCategoria();
        }

        /// <summary>
        /// Valida que la fecha de inicio no sea posterior a la fecha fin
        /// antes de consultar — evita una consulta inútil a la BD.
        /// </summary>
        public (List<ReporteVentasFecha> Datos, string Error) VentasPorRangoFechas(
            DateTime fechaInicio, DateTime fechaFin)
        {
            // Validación de rango lógico de fechas — requerimiento del proyecto
            if (fechaInicio > fechaFin)
                return (null, "La fecha de inicio no puede ser posterior a la fecha fin.");

            try
            {
                var datos = _repositorio.VentasPorRangoFechas(fechaInicio, fechaFin);
                return (datos, null);
            }
            catch (Exception ex)
            {
                return (null, $"Error al generar reporte: {ex.Message}");
            }
        }
    }
}