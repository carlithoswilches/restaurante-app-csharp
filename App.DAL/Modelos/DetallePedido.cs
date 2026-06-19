namespace App.DAL.Modelos
{
    public class DetallePedido
    {
        public DetallePedido() { }

        public int     IdDetalle      { get; set; }
        public int     IdPedido       { get; set; }
        public int     IdPlato        { get; set; }
        public string  NombrePlato    { get; set; }
        public int     Cantidad       { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal       { get; set; }
    }
}