using App.DAL.Modelos;
using App.DAL.Repositorios;
using System;
using System.Collections.Generic;

namespace App.BLL.Servicios
{
    /// <summary>
    /// Servicio de Mesas: valida reglas de negocio antes de delegar a DAL.
    /// </summary>
    public class MesaServicio
    {
        private readonly MesaRepositorio _repositorio;

        public MesaServicio()
        {
            _repositorio = new MesaRepositorio();
        }

        public List<Mesa> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public string Insertar(Mesa mesa)
        {
            // Número de mesa debe ser positivo
            if (mesa.NumeroMesa <= 0)
                return "El número de mesa debe ser mayor a cero.";

            // Capacidad mínima de 1 persona
            if (mesa.Capacidad <= 0)
                return "La capacidad debe ser mayor a cero.";

            if (mesa.Capacidad > 20)
                return "La capacidad no puede superar 20 personas.";

            if (string.IsNullOrWhiteSpace(mesa.Estado))
                return "El estado es obligatorio.";

            try
            {
                bool resultado = _repositorio.Insertar(mesa);
                return resultado ? null : "No se pudo guardar la mesa.";
            }
            catch (Exception ex)
            {
                return $"Error al guardar: {ex.Message}";
            }
        }

        public string Actualizar(Mesa mesa)
        {
            if (mesa.IdMesa <= 0)
                return "Debe seleccionar una mesa para editar.";

            if (mesa.NumeroMesa <= 0)
                return "El número de mesa debe ser mayor a cero.";

            if (mesa.Capacidad <= 0)
                return "La capacidad debe ser mayor a cero.";

            if (string.IsNullOrWhiteSpace(mesa.Estado))
                return "El estado es obligatorio.";

            try
            {
                bool resultado = _repositorio.Actualizar(mesa);
                return resultado ? null : "No se pudo actualizar la mesa.";
            }
            catch (Exception ex)
            {
                return $"Error al actualizar: {ex.Message}";
            }
        }

        public string Eliminar(int idMesa)
        {
            if (idMesa <= 0)
                return "ID de mesa inválido.";

            try
            {
                bool resultado = _repositorio.Eliminar(idMesa);
                return resultado ? null : "No se encontró la mesa o ya fue eliminada.";
            }
            catch (Exception ex)
            {
                return $"Error al eliminar: {ex.Message}";
            }
        }
    }
}