using Microsoft.Data.SqlClient;
using App.DAL.Conexion;
using App.DAL.Modelos;
using System;
using System.Collections.Generic;

namespace App.DAL.Repositorios
{
    /// <summary>
    /// Repositorio de Usuarios: autenticación y CRUD completo.
    /// Solo accesible para el perfil Administrador — se controla en UI.
    /// </summary>
    public class UsuarioRepositorio
    {
        /// <summary>
        /// Autentica un usuario comparando nombre y contraseña hash contra BD.
        /// Retorna el objeto Usuario si existe, null si no.
        /// </summary>
        public Usuario? Autenticar(string nombreUsuario, string contrasenaHash)
        {
            Usuario? usuario = null;

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                // JOIN con Perfiles para traer el nombre del perfil en una sola consulta
                string sql = @"SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto, 
                                      u.IdPerfil, p.NombrePerfil
                               FROM Usuarios u
                               INNER JOIN Perfiles p ON u.IdPerfil = p.IdPerfil
                               WHERE u.NombreUsuario = @NombreUsuario 
                                 AND u.Contrasena    = @Contrasena
                                 AND u.Activo        = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    cmd.Parameters.AddWithValue("@Contrasena",    contrasenaHash);

                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            //recorre lista y asigna su valor
                            if (reader.Read())
                            {
                                usuario = new Usuario
                                {
                                    Id            = reader.GetInt32(0),
                                    NombreUsuario = reader.GetString(1),
                                    Nombres       = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    IdPerfil      = reader.GetInt32(3),
                                    NombrePerfil  = reader.GetString(4)
                                };
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al autenticar: {ex.Message}");
                    }
                }
            }
            return usuario;
        }

        /// <summary>
        /// Obtiene todos los usuarios con su perfil — para el módulo de gestión.
        /// </summary>
        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto,
                                      u.IdPerfil, p.NombrePerfil, u.Activo
                               FROM Usuarios u
                               INNER JOIN Perfiles p ON u.IdPerfil = p.IdPerfil";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Usuario
                                {
                                    Id            = reader.GetInt32(0),
                                    NombreUsuario = reader.GetString(1),
                                    Nombres       = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    IdPerfil      = reader.GetInt32(3),
                                    NombrePerfil  = reader.GetString(4),
                                    Activo        = reader.GetBoolean(5)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al obtener usuarios: {ex.Message}");
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Inserta un nuevo usuario con contraseña ya encriptada.
        /// </summary>
        public bool Insertar(Usuario usuario)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"INSERT INTO Usuarios (NombreUsuario, Contrasena, NombreCompleto, IdPerfil, Activo)
                               VALUES (@NombreUsuario, @Contrasena, @NombreCompleto, @IdPerfil, @Activo)";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario",  usuario.NombreUsuario);
                    cmd.Parameters.AddWithValue("@Contrasena",     usuario.Contrasena);
                    cmd.Parameters.AddWithValue("@NombreCompleto", usuario.Nombres ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdPerfil",       usuario.IdPerfil);
                    cmd.Parameters.AddWithValue("@Activo",         usuario.Activo);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al insertar usuario: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Actualiza datos del usuario. Si la contraseña viene vacía, no la cambia.
        /// </summary>
        public bool Actualizar(Usuario usuario, bool cambiarContrasena)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                // Si cambiarContrasena es false, el UPDATE no toca la columna Contrasena
                string sql = cambiarContrasena
                    ? @"UPDATE Usuarios 
                        SET NombreUsuario  = @NombreUsuario,
                            Contrasena     = @Contrasena,
                            NombreCompleto = @NombreCompleto,
                            IdPerfil       = @IdPerfil,
                            Activo         = @Activo
                        WHERE IdUsuario = @IdUsuario"
                    : @"UPDATE Usuarios 
                        SET NombreUsuario  = @NombreUsuario,
                            NombreCompleto = @NombreCompleto,
                            IdPerfil       = @IdPerfil,
                            Activo         = @Activo
                        WHERE IdUsuario = @IdUsuario";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario",      usuario.Id);
                    cmd.Parameters.AddWithValue("@NombreUsuario",  usuario.NombreUsuario);
                    cmd.Parameters.AddWithValue("@NombreCompleto", usuario.Nombres ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdPerfil",       usuario.IdPerfil);
                    cmd.Parameters.AddWithValue("@Activo",         usuario.Activo);

                    if (cambiarContrasena)
                        cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al actualizar usuario: {ex.Message}");
                    }
                }
            }
        }

        public bool Eliminar(int idUsuario)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = "DELETE FROM Usuarios WHERE IdUsuario = @IdUsuario";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al eliminar usuario: {ex.Message}");
                    }
                }
            }
        }

       /// <summary>
        /// Obtiene todos los perfiles para cargar el ComboBox.
        /// Retorna List<Perfil> en lugar de tuplas para compatibilidad con DataBinding.
        /// </summary>
        public List<Perfil> ObtenerPerfiles()
        {
            var lista = new List<Perfil>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = "SELECT IdPerfil, NombrePerfil FROM Perfiles";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Perfil
                                {
                                    Id     = reader.GetInt32(0),
                                    Nombre = reader.GetString(1)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al obtener perfiles: {ex.Message}");
                    }
                }
            }
            return lista;
        }
    }
}