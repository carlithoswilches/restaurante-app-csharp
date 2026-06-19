using Microsoft.Data.SqlClient;
using App.DAL.Conexion;
using App.DAL.Modelos;
using System;
using System.Collections.Generic;

namespace App.DAL.Repositorios
{
    /// <summary>
    /// Repositorio de Clientes: ejecuta todas las consultas SQL
    /// sobre la tabla Clientes.
    /// </summary>
    public class ClienteRepositorio
    {
        public List<Cliente> ObtenerTodos()
        {
            List<Cliente> lista = new List<Cliente>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"SELECT IdCliente, Nombres, Apellidos, 
                                      Documento, Telefono, Email, Activo 
                               FROM Clientes";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Cliente
                                {
                                    Id        = reader.GetInt32(0),
                                    Nombres   = reader.GetString(1),
                                    Apellidos = reader.GetString(2),
                                    Documento = reader.GetString(3),
                                    Telefono  = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    Email     = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    Activo    = reader.GetBoolean(6)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al obtener clientes: {ex.Message}");
                    }
                }
            }
            return lista;
        }

        public bool Insertar(Cliente cliente)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"INSERT INTO Clientes (Nombres, Apellidos, Documento, Telefono, Email, Activo)
                               VALUES (@Nombres, @Apellidos, @Documento, @Telefono, @Email, @Activo)";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@Nombres",   cliente.Nombres);
                    cmd.Parameters.AddWithValue("@Apellidos", cliente.Apellidos);
                    cmd.Parameters.AddWithValue("@Documento", cliente.Documento);
                    cmd.Parameters.AddWithValue("@Telefono",  cliente.Telefono  ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email",     cliente.Email     ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo",    cliente.Activo);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al insertar cliente: {ex.Message}");
                    }
                }
            }
        }

        public bool Actualizar(Cliente cliente)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"UPDATE Clientes 
                               SET Nombres   = @Nombres,
                                   Apellidos = @Apellidos,
                                   Documento = @Documento,
                                   Telefono  = @Telefono,
                                   Email     = @Email,
                                   Activo    = @Activo
                               WHERE IdCliente = @IdCliente";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdCliente", cliente.Id);
                    cmd.Parameters.AddWithValue("@Nombres",   cliente.Nombres);
                    cmd.Parameters.AddWithValue("@Apellidos", cliente.Apellidos);
                    cmd.Parameters.AddWithValue("@Documento", cliente.Documento);
                    cmd.Parameters.AddWithValue("@Telefono",  cliente.Telefono  ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email",     cliente.Email     ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo",    cliente.Activo);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al actualizar cliente: {ex.Message}");
                    }
                }
            }
        }

        public bool Eliminar(int idCliente)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = "DELETE FROM Clientes WHERE IdCliente = @IdCliente";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al eliminar cliente: {ex.Message}");
                    }
                }
            }
        }

        public List<Cliente> Buscar(string criterio)
        {
            List<Cliente> lista = new List<Cliente>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"SELECT IdCliente, Nombres, Apellidos, Documento, Telefono, Email, Activo
                               FROM Clientes
                               WHERE Nombres   LIKE @Criterio 
                                  OR Apellidos LIKE @Criterio
                                  OR Documento LIKE @Criterio";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@Criterio", $"%{criterio}%");

                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Cliente
                                {
                                    Id        = reader.GetInt32(0),
                                    Nombres   = reader.GetString(1),
                                    Apellidos = reader.GetString(2),
                                    Documento = reader.GetString(3),
                                    Telefono  = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    Email     = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    Activo    = reader.GetBoolean(6)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al buscar clientes: {ex.Message}");
                    }
                }
            }
            return lista;
        }
    }
}