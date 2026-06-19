using App.DAL.Modelos;

namespace App.BLL.Servicios
{
    /// <summary>
    /// Clase estática que almacena el usuario actualmente logueado.
    /// Al ser estática, su información es accesible desde cualquier formulario
    /// sin necesidad de pasar el usuario como parámetro entre ventanas.
    /// </summary>
    public static class Sesion
    {
        // Usuario activo — null si no hay nadie logueado
        public static Usuario UsuarioActual { get; set; }

        /// <summary>
        /// Verifica si el usuario logueado tiene el perfil indicado.
        /// Uso: Sesion.TienePerfil("Administrador")
        /// </summary>
        public static bool TienePerfil(string nombrePerfil)
        {
            return UsuarioActual?.NombrePerfil == nombrePerfil;
        }

        /// <summary>
        /// Cierra la sesión limpiando el usuario actual.
        /// Se llama al cerrar sesión o al cerrar el MDI.
        /// </summary>
        public static void CerrarSesion()
        {
            UsuarioActual = null;
        }
    }
}