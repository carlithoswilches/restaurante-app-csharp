namespace App.DAL.Modelos
{
    public class Cliente : Persona
    {
        public Cliente() { }

        public Cliente(string nombres, string apellidos, string documento, 
                       string telefono, string email)
            : base(nombres, apellidos, documento, telefono)
        {
            Email = email;
        }

        public string Email { get; set; }

        // Override - sobreescribe el método de Persona (esto es polimorfismo)
        //"Cliente muestra el email además del nombre"
        public override string ObtenerNombreCompleto()
        {
            return $"{Nombres} {Apellidos} ({Email})";
        }
    }
}