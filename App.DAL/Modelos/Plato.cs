namespace App.DAL.Modelos
{
    
    public class Plato
    {

        // Propiedades - cada una corresponde a una columna de la tabla Platos
        public int     IdPlato     { get; set; }
        public string  NombrePlato { get; set; }
        public string  Categoria   { get; set; }
        public decimal Precio      { get; set; }
        public string  Descripcion { get; set; }
        public bool    Disponible  { get; set; }
        
        //Creamos contructor vacio , es nececsario para crear objetos sin datos iniciales
        public Plato(){}

        //Constructor con parametros, para crear un plato con todos sus parametros
        public Plato(int idPlato, string nombrePlato, string categoria, 
                     decimal precio, string descripcion, bool disponible)
        {
            IdPlato      = idPlato;
            NombrePlato  = nombrePlato;
            Categoria    = categoria;
            Precio       = precio;
            Descripcion  = descripcion;
            Disponible   = disponible;
        }

        
    }
}