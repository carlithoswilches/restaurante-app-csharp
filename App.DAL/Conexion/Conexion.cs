using Microsoft.Data.SqlClient;
using System.Configuration;

namespace App.DAL.Conexion
{
    /// Clase estática responsable de proveer la conexión a SQL Server.
    /// Al ser estática, no necesita instanciarse — se usa directamente como Conexion.ObtenerConexion()
    public static class Conexion
    {
        // Cadena de conexión almacenada en App.Config — nunca hardcodeada en el código
        // ConfigurationManager lee el archivo de configuración en tiempo de ejecución
        private static readonly string _cadenaConexion = 
            ConfigurationManager.ConnectionStrings["RestauranteDB"].ConnectionString;

        /// Retorna un objeto SqlConnection listo para abrir.
        /// Cada repositorio llama este método para obtener su conexión
        public static SqlConnection ObtenerConexion()
        {
            // SqlConnection representa una conexión física a SQL Server
            // Recibe la cadena de conexión como parámetro
            return new SqlConnection(_cadenaConexion);
        }
    }
}