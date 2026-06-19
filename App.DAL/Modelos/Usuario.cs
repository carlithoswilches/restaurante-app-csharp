namespace App.DAL.Modelos
{
    public class Usuario : Persona
    {
        public Usuario() { }

        public Usuario(string nombres, string apellidos, string documento,
                       string telefono, string nombreUsuario, int idPerfil)
            : base(nombres, apellidos, documento, telefono)
        {
            NombreUsuario = nombreUsuario;
            IdPerfil      = idPerfil;
        }

        public string NombreUsuario { get; set; }
        public string Contrasena   { get; set; }
        public int    IdPerfil     { get; set; }
        public string NombrePerfil { get; set; }

        // Override - Usuario muestra su nombre de usuario
        public override string ObtenerNombreCompleto()
        {
            return $"{Nombres} {Apellidos} [{NombreUsuario}]";
        }
    }
}