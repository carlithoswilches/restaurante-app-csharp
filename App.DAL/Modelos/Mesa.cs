namespace App.DAL.Modelos
{
    public class Mesa
    {
        public Mesa() { }

        public int    IdMesa     { get; set; }
        public int    NumeroMesa { get; set; }
        public int    Capacidad  { get; set; }
        public string Estado     { get; set; }
        public string Ubicacion  { get; set; }
    }
}