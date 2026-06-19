using App.DAL.Modelos;
using App.DAL.Repositorios;
using System;
using System.Collections.Generic;

namespace App.BLL.Servicios
{
    /// Capa de Lógica de Negocio para Platos.
    /// Responsabilidad: validar los datos ANTES de enviarlos a DAL.
    /// La UI llama a esta clase, nunca directamente al repositorio.
  
    public class PlatoServicio
    {
        // Instancia del repositorio — BLL depende de DAL pero UI no lo sabe
        // La UI solo conoce PlatoServicio, no sabe que existe PlatoRepositorio
        private readonly PlatoRepositorio _repositorio;

        /// Constructor: inicializa el repositorio que usará este servicio.
        /// 'readonly' significa que _repositorio no puede reasignarse después del constructor.
        public PlatoServicio()
        {
            _repositorio = new PlatoRepositorio();
        }

        /// Retorna todos los platos. No necesita validación — es solo una consulta.
        public List<Plato> ObtenerTodos()
        {
            // Delega directamente al repositorio — no hay reglas de negocio aquí
            return _repositorio.ObtenerTodos();
        }

        /// Valida y guarda un nuevo plato.
        /// Retorna un string con el error si algo falla, o null si todo está bien.
        /// Esto permite que la UI muestre el mensaje exacto al usuario.
        public string Insertar(Plato plato)
        {
            // ── Validaciones de negocio ──────────────────────────────────────

            // string.IsNullOrWhiteSpace: verifica null, vacío o solo espacios
            if (string.IsNullOrWhiteSpace(plato.NombrePlato))
                return "El nombre del plato es obligatorio.";

            // Trim() elimina espacios al inicio y al final del texto
            if (plato.NombrePlato.Trim().Length < 3)
                return "El nombre debe tener mínimo 3 caracteres.";

            if (string.IsNullOrWhiteSpace(plato.Categoria))
                return "La categoría es obligatoria.";

            // Regla de negocio: precio no puede ser negativo ni cero
            if (plato.Precio <= 0)
                return "El precio debe ser mayor a cero.";

            // Regla de negocio: precio máximo razonable para un plato
            if (plato.Precio > 500000)
                return "El precio no puede superar $500.000.";

            // ── Si pasa todas las validaciones, delega a DAL ─────────────────
            try
            {
                bool resultado = _repositorio.Insertar(plato);

                // Si DAL retornó false, significa 0 filas afectadas — algo salió mal
                if (!resultado)
                    return "No se pudo guardar el plato. Intente de nuevo.";

                // null significa éxito — la UI lo interpreta como "sin errores"
                return null;
            }
            catch (Exception ex)
            {
                // Captura el error que lanzó DAL y lo convierte en mensaje amigable
                // ex.Message puede contener detalles técnicos útiles para depuración
                return $"Error al guardar: {ex.Message}";
            }
        }

        /// Valida y actualiza un plato existente.
        /// Mismas reglas que Insertar más verificación de ID válido.
        public string Actualizar(Plato plato)
        {
            // Un ID válido siempre es mayor a 0 — los IDENTITY en SQL empiezan en 1
            if (plato.IdPlato <= 0)
                return "Debe seleccionar un plato para editar.";

            if (string.IsNullOrWhiteSpace(plato.NombrePlato))
                return "El nombre del plato es obligatorio.";

            if (plato.NombrePlato.Trim().Length < 3)
                return "El nombre debe tener mínimo 3 caracteres.";

            if (string.IsNullOrWhiteSpace(plato.Categoria))
                return "La categoría es obligatoria.";

            if (plato.Precio <= 0)
                return "El precio debe ser mayor a cero.";

            if (plato.Precio > 500000)
                return "El precio no puede superar $500.000.";

            try
            {
                bool resultado = _repositorio.Actualizar(plato);

                if (!resultado)
                    return "No se pudo actualizar el plato. Intente de nuevo.";

                return null;
            }
            catch (Exception ex)
            {
                return $"Error al actualizar: {ex.Message}";
            }
        }

        /// Elimina un plato por ID.
        /// La confirmación "¿Está seguro?" se hace en la UI, no aquí.
        /// BLL solo valida que el ID sea válido.
        public string Eliminar(int idPlato)
        {
            if (idPlato <= 0)
                return "ID de plato inválido.";

            try
            {
                bool resultado = _repositorio.Eliminar(idPlato);

                if (!resultado)
                    return "No se encontró el plato o ya fue eliminado.";

                return null;
            }
            catch (Exception ex)
            {
                return $"Error al eliminar: {ex.Message}";
            }
        }

        /// Busca platos por nombre o categoría.
        /// Si el criterio está vacío retorna todos — comportamiento esperado en un buscador.
        public List<Plato> Buscar(string criterio)
        {
            // Si no escribe nada en el buscador, muestra todo
            if (string.IsNullOrWhiteSpace(criterio))
                return _repositorio.ObtenerTodos();

            return _repositorio.Buscar(criterio.Trim());
        }
    }
}