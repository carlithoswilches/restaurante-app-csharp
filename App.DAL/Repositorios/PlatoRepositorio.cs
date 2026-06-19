using Microsoft.Data.SqlClient;
using App.DAL.Conexion;
using App.DAL.Modelos;
using System;
using System.Collections.Generic;

namespace App.DAL.Repositorios
{
    /// Repositorio de Platos: contiene todos los métodos que ejecutan
    /// consultas SQL sobre la tabla Platos.
    /// Patrón Repository: aísla el acceso a datos del resto de la aplicación.
    public class PlatoRepositorio
    {
        /// Obtiene todos los platos registrados en la BD.
        /// Retorna List<Plato> — colección genérica tipada, más segura que ArrayList.
        public List<Plato> ObtenerTodos()
        {
            // List<T>: colección genérica que solo acepta objetos de tipo Plato
            List<Plato> lista = new List<Plato>();

            // 'using' garantiza que la conexión se cierre aunque ocurra una excepción
            // Equivale a un try-finally que llama connection.Close() automáticamente
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                // Consulta SQL — SELECT trae todas las columnas de todos los platos
                string sql = "SELECT IdPlato, NombrePlato, Categoria, Precio, Descripcion, Disponible FROM Platos";

                // SqlCommand: representa el comando SQL que se va a ejecutar
                // Recibe el SQL y la conexión como parámetros
                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        // Abre la conexión física con SQL Server
                        conexion.Open();

                        // SqlDataReader: lee los resultados fila por fila
                       
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Read() avanza al siguiente registro — retorna false cuando no hay más
                            while (reader.Read())
                            {
                                // Mapeo: cada columna del reader se asigna a una propiedad del objeto
                                // GetInt32, GetString, GetDecimal, GetBoolean según el tipo de dato
                                Plato plato = new Plato
                                {
                                    IdPlato     = reader.GetInt32(0),    // columna 0: IdPlato
                                    NombrePlato = reader.GetString(1),   // columna 1: NombrePlato
                                    Categoria   = reader.GetString(2),   // columna 2: Categoria
                                    Precio      = reader.GetDecimal(3),  // columna 3: Precio
                                    // IsDBNull verifica si el valor es NULL en BD antes de leerlo
                                    Descripcion = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    Disponible  = reader.GetBoolean(5)   // columna 5: Disponible
                                };

                                // Agrega el plato construido a la lista
                                lista.Add(plato);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // SqlException: excepción específica de SQL Server
                        // En producción se loggea, aquí la relanzamos para que BLL la maneje
                        throw new Exception($"Error al obtener platos: {ex.Message}");
                    }
                }
            }

            return lista;
        }

        /// Inserta un nuevo plato en la BD.
        /// Usa parámetros SQL (@param) para prevenir inyección SQL — en la practica NUNCA hay que concatenar strings.
        public bool Insertar(Plato plato)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                // @NombrePlato, @Categoria, etc. son parámetros — SQL Server los reemplaza
                // de forma segura, sin riesgo de inyección SQL
                string sql = @"INSERT INTO Platos (NombrePlato, Categoria, Precio, Descripcion, Disponible)
                               VALUES (@NombrePlato, @Categoria, @Precio, @Descripcion, @Disponible)";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    // AddWithValue: asigna el valor al parámetro SQL
                    // El tipo de dato se infiere automáticamente del valor de C#
                    cmd.Parameters.AddWithValue("@NombrePlato", plato.NombrePlato);
                    cmd.Parameters.AddWithValue("@Categoria",   plato.Categoria);
                    cmd.Parameters.AddWithValue("@Precio",      plato.Precio);
                    //En C#, la ausencia de datos se escribe como null. Pero SQL Server no entiende el null de C#; 
                    //SQL Server usa su propio concepto llamado DBNull.
                    cmd.Parameters.AddWithValue("@Descripcion", plato.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Disponible",  plato.Disponible);

                    try
                    {
                        conexion.Open();

                        // ExecuteNonQuery: ejecuta INSERT, UPDATE, DELETE
                        // Retorna el número de filas afectadas
                        int filasAfectadas = cmd.ExecuteNonQuery();

                        // Si afectó al menos 1 fila, la inserción fue exitosa
                        return filasAfectadas > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al insertar plato: {ex.Message}");
                    }
                }
            }
        }

        /// Actualiza los datos de un plato existente identificado por su IdPlato.
        public bool Actualizar(Plato plato)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"UPDATE Platos 
                               SET NombrePlato = @NombrePlato,
                                   Categoria   = @Categoria,
                                   Precio      = @Precio,
                                   Descripcion = @Descripcion,
                                   Disponible  = @Disponible
                               WHERE IdPlato = @IdPlato";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdPlato",     plato.IdPlato);
                    cmd.Parameters.AddWithValue("@NombrePlato", plato.NombrePlato);
                    cmd.Parameters.AddWithValue("@Categoria",   plato.Categoria);
                    cmd.Parameters.AddWithValue("@Precio",      plato.Precio);
                    cmd.Parameters.AddWithValue("@Descripcion", plato.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Disponible",  plato.Disponible);

                    try
                    {
                        conexion.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al actualizar plato: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Elimina un plato de la BD por su ID.
        /// En sistemas reales se prefiere eliminación lógica (Disponible = false)
        /// pero aquí implementamos eliminación física según el requerimiento.
        /// </summary>
        public bool Eliminar(int idPlato)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = "DELETE FROM Platos WHERE IdPlato = @IdPlato";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdPlato", idPlato);

                    try
                    {
                        conexion.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al eliminar plato: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Busca platos por nombre o categoría — para el filtro del DataGridView.
        /// Usa LIKE con % para búsqueda parcial: "pollo" encuentra "Pollo a la plancha".
        /// </summary>
        public List<Plato> Buscar(string criterio)
        {
            List<Plato> lista = new List<Plato>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                //Tráeme todos los platos cuya nombre o categoría se parezca a lo que yo le pase como criterio
                string sql = @"SELECT IdPlato, NombrePlato, Categoria, Precio, Descripcion, Disponible 
                               FROM Platos
                               WHERE NombrePlato LIKE @Criterio OR Categoria LIKE @Criterio";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    // % es comodín en SQL: %pollo% encuentra cualquier texto que contenga "pollo"
                    cmd.Parameters.AddWithValue("@Criterio", $"%{criterio}%");

                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Plato
                                {
                                    IdPlato     = reader.GetInt32(0),
                                    NombrePlato = reader.GetString(1),
                                    Categoria   = reader.GetString(2),
                                    Precio      = reader.GetDecimal(3),
                                    Descripcion = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    Disponible  = reader.GetBoolean(5)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al buscar platos: {ex.Message}");
                    }
                }
            }

            return lista;
        }
    }
}