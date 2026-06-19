using System;
using System.Collections.Generic;

namespace App.DAL.Modelos
{
    public class Pedido
    {
        public Pedido() 
        { 
            // Lista para los platos del pedido - aquí usamos List<T>
            Detalle = new List<DetallePedido>();
        }

        public int              IdPedido    { get; set; }
        public int              IdMesa      { get; set; }
        public int              IdCliente   { get; set; }
        public int              IdUsuario   { get; set; }
        public DateTime         FechaPedido { get; set; }
        public string           Estado      { get; set; }
        public decimal          Total       { get; set; }
        public List<DetallePedido> Detalle  { get; set; }

        // Nombres para mostrar en UI (no son columnas de BD, son joins)
        public string NombreMesa    { get; set; }
        public string NombreCliente { get; set; }
    }
}