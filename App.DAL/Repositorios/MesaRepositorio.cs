using Microsoft.Data.SqlClient;
using App.DAL.Conexion;
using App.DAL.Modelos;
using System;
using System.Collections.Generic;

namespace App.DAL.Repositorios
{
    /// <summary>
    /// Repositorio de Mesas: ejecuta todas las consultas SQL
    /// sobre la tabla Mesas. Mismo patrón que PlatoRepositorio.
    /// </summary>
    public class MesaRepositorio
    {
        public List<Mesa> ObtenerTodos()
        {
            List<Mesa> lista = new List<Mesa>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = "SELECT IdMesa, NumeroMesa, Capacidad, Estado, Ubicacion FROM Mesas";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Mesa
                                {
                                    IdMesa     = reader.GetInt32(0),
                                    NumeroMesa = reader.GetInt32(1),
                                    Capacidad  = reader.GetInt32(2),
                                    Estado     = reader.GetString(3),
                                    Ubicacion  = reader.IsDBNull(4) ? "" : reader.GetString(4)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al obtener mesas: {ex.Message}");
                    }
                }
            }
            return lista;
        }

        public bool Insertar(Mesa mesa)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"INSERT INTO Mesas (NumeroMesa, Capacidad, Estado, Ubicacion)
                               VALUES (@NumeroMesa, @Capacidad, @Estado, @Ubicacion)";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@NumeroMesa", mesa.NumeroMesa);
                    cmd.Parameters.AddWithValue("@Capacidad",  mesa.Capacidad);
                    cmd.Parameters.AddWithValue("@Estado",     mesa.Estado);
                    cmd.Parameters.AddWithValue("@Ubicacion",  mesa.Ubicacion ?? (object)DBNull.Value);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al insertar mesa: {ex.Message}");
                    }
                }
            }
        }

        public bool Actualizar(Mesa mesa)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"UPDATE Mesas 
                               SET NumeroMesa = @NumeroMesa,
                                   Capacidad  = @Capacidad,
                                   Estado     = @Estado,
                                   Ubicacion  = @Ubicacion
                               WHERE IdMesa = @IdMesa";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdMesa",     mesa.IdMesa);
                    cmd.Parameters.AddWithValue("@NumeroMesa", mesa.NumeroMesa);
                    cmd.Parameters.AddWithValue("@Capacidad",  mesa.Capacidad);
                    cmd.Parameters.AddWithValue("@Estado",     mesa.Estado);
                    cmd.Parameters.AddWithValue("@Ubicacion",  mesa.Ubicacion ?? (object)DBNull.Value);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al actualizar mesa: {ex.Message}");
                    }
                }
            }
        }

        public bool Eliminar(int idMesa)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = "DELETE FROM Mesas WHERE IdMesa = @IdMesa";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdMesa", idMesa);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al eliminar mesa: {ex.Message}");
                    }
                }
            }
        }
    }
}