namespace App.DAL.Modelos
{
    // Clase base - abstracta porque nunca crearás una "Persona" sola, 
    // siempre será un Cliente o un Usuario
    public abstract class Persona
    {
        public Persona() { }

        //Constructor con parametros, para crear un plato con todos sus parametros
        public Persona(string nombres, string apellidos, string documento, string telefono)
        {
            Nombres    = nombres;
            Apellidos  = apellidos;
            Documento  = documento;
            Telefono   = telefono;
        }

        public int    Id        { get; set; }
        public string Nombres   { get; set; }
        public string Apellidos { get; set; }
        public string Documento { get; set; }
        public string Telefono  { get; set; }
        public bool   Activo    { get; set; } = true;

        // Método virtual - las clases hijas pueden sobreescribirlo (polimorfismo)
        public virtual string ObtenerNombreCompleto()
        {
            return $"{Nombres} {Apellidos}";
        }
    }
}