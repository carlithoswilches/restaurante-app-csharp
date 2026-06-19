using App.DAL.Modelos;
using App.DAL.Repositorios;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace App.BLL.Servicios
{
    /// <summary>
    /// Servicio de Clientes: valida reglas de negocio.
    /// Incluye validación de email con Regex — requerimiento del proyecto.
    /// </summary>
    public class ClienteServicio
    {
        private readonly ClienteRepositorio _repositorio;

        public ClienteServicio()
        {
            _repositorio = new ClienteRepositorio();
        }

        public List<Cliente> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Cliente> Buscar(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                return _repositorio.ObtenerTodos();

            return _repositorio.Buscar(criterio.Trim());
        }

        public string Insertar(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombres))
                return "El nombre es obligatorio.";

            if (string.IsNullOrWhiteSpace(cliente.Apellidos))
                return "Los apellidos son obligatorios.";

            if (string.IsNullOrWhiteSpace(cliente.Documento))
                return "El documento es obligatorio.";

            if (cliente.Documento.Length < 6)
                return "El documento debe tener mínimo 6 caracteres.";

            // Validación de email con Regex — requerimiento explícito del proyecto
            // El patrón verifica que tenga @ y al menos un punto después
            if (!string.IsNullOrWhiteSpace(cliente.Email))
            {
                bool emailValido = Regex.IsMatch(cliente.Email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailValido)
                    return "El formato del email no es válido.";
            }

            try
            {
                bool resultado = _repositorio.Insertar(cliente);
                return resultado ? null : "No se pudo guardar el cliente.";
            }
            catch (Exception ex)
            {
                return $"Error al guardar: {ex.Message}";
            }
        }

        public string Actualizar(Cliente cliente)
        {
            if (cliente.Id <= 0)
                return "Debe seleccionar un cliente para editar.";

            if (string.IsNullOrWhiteSpace(cliente.Nombres))
                return "El nombre es obligatorio.";

            if (string.IsNullOrWhiteSpace(cliente.Apellidos))
                return "Los apellidos son obligatorios.";

            if (string.IsNullOrWhiteSpace(cliente.Documento))
                return "El documento es obligatorio.";

            if (!string.IsNullOrWhiteSpace(cliente.Email))
            {
                bool emailValido = Regex.IsMatch(cliente.Email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailValido)
                    return "El formato del email no es válido.";
            }

            try
            {
                bool resultado = _repositorio.Actualizar(cliente);
                return resultado ? null : "No se pudo actualizar el cliente.";
            }
            catch (Exception ex)
            {
                return $"Error al actualizar: {ex.Message}";
            }
        }

        public string Eliminar(int idCliente)
        {
            if (idCliente <= 0)
                return "ID de cliente inválido.";

            try
            {
                bool resultado = _repositorio.Eliminar(idCliente);
                return resultado ? null : "No se encontró el cliente o ya fue eliminado.";
            }
            catch (Exception ex)
            {
                return $"Error al eliminar: {ex.Message}";
            }
        }
    }
}