using App.DAL.Modelos;
using App.DAL.Repositorios;
using System;
using System.Collections.Generic;

namespace App.BLL.Servicios
{
    /// <summary>
    /// Servicio de Pedidos: valida que el pedido tenga mesa, cliente y al menos un plato.
    /// </summary>
    public class PedidoServicio
    {
        private readonly PedidoRepositorio _repositorio;

        public PedidoServicio()
        {
            _repositorio = new PedidoRepositorio();
        }

        public List<Pedido> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<DetallePedido> ObtenerDetalle(int idPedido)
        {
            return _repositorio.ObtenerDetalle(idPedido);
        }

        public string Insertar(Pedido pedido)
        {
            // Validaciones de negocio
            if (pedido.IdMesa <= 0)
                return "Debe seleccionar una mesa.";

            if (pedido.IdCliente <= 0)
                return "Debe seleccionar un cliente.";

            // Un pedido sin platos no tiene sentido
            if (pedido.Detalle == null || pedido.Detalle.Count == 0)
                return "Debe agregar al menos un plato al pedido.";

            // Calcula el total sumando los subtotales del detalle
            pedido.Total = 0;
            foreach (DetallePedido d in pedido.Detalle)
                pedido.Total += d.Cantidad * d.PrecioUnitario;

            try
            {
                bool resultado = _repositorio.Insertar(pedido);
                return resultado ? null : "No se pudo guardar el pedido.";
            }
            catch (Exception ex)
            {
                return $"Error al guardar: {ex.Message}";
            }
        }

        public string ActualizarEstado(int idPedido, string nuevoEstado)
        {
            if (idPedido <= 0)
                return "ID de pedido inválido.";

            if (string.IsNullOrWhiteSpace(nuevoEstado))
                return "El estado es obligatorio.";

            try
            {
                bool resultado = _repositorio.ActualizarEstado(idPedido, nuevoEstado);
                return resultado ? null : "No se pudo actualizar el estado.";
            }
            catch (Exception ex)
            {
                return $"Error al actualizar: {ex.Message}";
            }
        }

        public string Eliminar(int idPedido)
        {
            if (idPedido <= 0)
                return "ID de pedido inválido.";

            try
            {
                bool resultado = _repositorio.Eliminar(idPedido);
                return resultado ? null : "No se encontró el pedido.";
            }
            catch (Exception ex)
            {
                return $"Error al eliminar: {ex.Message}";
            }
        }
    }
}