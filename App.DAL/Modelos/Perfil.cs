namespace App.DAL.Modelos
{
    /// <summary>
    /// Clase simple para representar un perfil.
    /// Necesaria porque Windows Forms DataBinding no funciona
    /// correctamente con tuplas — requiere propiedades nombradas en una clase.
    /// </summary>
    public class Perfil
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; }
    }
}