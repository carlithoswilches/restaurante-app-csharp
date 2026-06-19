using Microsoft.Data.SqlClient;
using App.DAL.Conexion;
using App.DAL.Modelos;
using System;
using System.Collections.Generic;

namespace App.DAL.Repositorios
{
    /// <summary>
    /// Repositorio de Pedidos: maneja la cabecera del pedido y su detalle.
    /// Una transacción SQL garantiza que si falla el detalle, 
    /// también se revierte la cabecera — atomicidad.
    /// </summary>
    public class PedidoRepositorio
    {
        public List<Pedido> ObtenerTodos()
        {
            List<Pedido> lista = new List<Pedido>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                // JOIN con Mesas y Clientes para mostrar nombres en lugar de IDs
                string sql = @"SELECT p.IdPedido, p.IdMesa, m.NumeroMesa,
                                      p.IdCliente, c.Nombres + ' ' + c.Apellidos AS NombreCliente,
                                      p.FechaPedido, p.Estado, p.Total
                               FROM Pedidos p
                               INNER JOIN Mesas    m ON p.IdMesa    = m.IdMesa
                               INNER JOIN Clientes c ON p.IdCliente = c.IdCliente
                               ORDER BY p.FechaPedido DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new Pedido
                                {
                                    IdPedido      = reader.GetInt32(0),
                                    IdMesa        = reader.GetInt32(1),
                                    NombreMesa    = $"Mesa {reader.GetInt32(2)}",
                                    IdCliente     = reader.GetInt32(3),
                                    NombreCliente = reader.GetString(4),
                                    FechaPedido   = reader.GetDateTime(5),
                                    Estado        = reader.GetString(6),
                                    Total         = reader.GetDecimal(7)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al obtener pedidos: {ex.Message}");
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Obtiene el detalle de un pedido específico — los platos que contiene.
        /// </summary>
        public List<DetallePedido> ObtenerDetalle(int idPedido)
        {
            List<DetallePedido> lista = new List<DetallePedido>();

            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = @"SELECT d.IdDetalle, d.IdPedido, d.IdPlato,
                                      pl.NombrePlato, d.Cantidad, d.PrecioUnitario, d.Subtotal
                               FROM DetallePedido d
                               INNER JOIN Platos pl ON d.IdPlato = pl.IdPlato
                               WHERE d.IdPedido = @IdPedido";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);

                    try
                    {
                        conexion.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lista.Add(new DetallePedido
                                {
                                    IdDetalle      = reader.GetInt32(0),
                                    IdPedido       = reader.GetInt32(1),
                                    IdPlato        = reader.GetInt32(2),
                                    NombrePlato    = reader.GetString(3),
                                    Cantidad       = reader.GetInt32(4),
                                    PrecioUnitario = reader.GetDecimal(5),
                                    Subtotal       = reader.GetDecimal(6)
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al obtener detalle: {ex.Message}");
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Inserta el pedido y su detalle en una transacción SQL.
        /// Transacción: si cualquier INSERT falla, se revierten TODOS los cambios.
        /// Esto garantiza que nunca quede un pedido sin detalle en BD.
        /// </summary>
        public bool Insertar(Pedido pedido)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                conexion.Open();

                // SqlTransaction: agrupa múltiples operaciones en una unidad atómica
                SqlTransaction transaccion = conexion.BeginTransaction();

                try
                {
                    // Paso 1: insertar la cabecera del pedido
                    // OUTPUT INSERTED.IdPedido retorna el ID generado por IDENTITY
                    string sqlPedido = @"INSERT INTO Pedidos (IdMesa, IdCliente, IdUsuario, Estado, Total)
                                        OUTPUT INSERTED.IdPedido
                                        VALUES (@IdMesa, @IdCliente, @IdUsuario, @Estado, @Total)";

                    int idPedidoGenerado;

                    using (SqlCommand cmd = new SqlCommand(sqlPedido, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@IdMesa",    pedido.IdMesa);
                        cmd.Parameters.AddWithValue("@IdCliente", pedido.IdCliente);
                        cmd.Parameters.AddWithValue("@IdUsuario", pedido.IdUsuario);
                        cmd.Parameters.AddWithValue("@Estado",    pedido.Estado);
                        cmd.Parameters.AddWithValue("@Total",     pedido.Total);

                        // ExecuteScalar: retorna un solo valor — el ID generado
                        idPedidoGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Paso 2: insertar cada línea del detalle con el ID generado
                    string sqlDetalle = @"INSERT INTO DetallePedido (IdPedido, IdPlato, Cantidad, PrecioUnitario)
                                         VALUES (@IdPedido, @IdPlato, @Cantidad, @PrecioUnitario)";

                    foreach (DetallePedido detalle in pedido.Detalle)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlDetalle, conexion, transaccion))
                        {
                            cmd.Parameters.AddWithValue("@IdPedido",       idPedidoGenerado);
                            cmd.Parameters.AddWithValue("@IdPlato",        detalle.IdPlato);
                            cmd.Parameters.AddWithValue("@Cantidad",       detalle.Cantidad);
                            cmd.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Confirma todos los cambios — hace permanentes los INSERT
                    transaccion.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    // Si algo falla, revierte todos los INSERT — BD queda intacta
                    transaccion.Rollback();
                    throw new Exception($"Error al insertar pedido: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Actualiza solo el estado del pedido — Pendiente, En preparación, Entregado.
        /// </summary>
        public bool ActualizarEstado(int idPedido, string nuevoEstado)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                string sql = "UPDATE Pedidos SET Estado = @Estado WHERE IdPedido = @IdPedido";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@Estado",   nuevoEstado);
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);

                    try
                    {
                        conexion.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Error al actualizar estado: {ex.Message}");
                    }
                }
            }
        }

        public bool Eliminar(int idPedido)
        {
            using (SqlConnection conexion = Conexion.Conexion.ObtenerConexion())
            {
                conexion.Open();
                SqlTransaction transaccion = conexion.BeginTransaction();

                try
                {
                    // Primero elimina el detalle — FK no permite eliminar cabecera primero
                    string sqlDetalle = "DELETE FROM DetallePedido WHERE IdPedido = @IdPedido";
                    using (SqlCommand cmd = new SqlCommand(sqlDetalle, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                        cmd.ExecuteNonQuery();
                    }

                    // Luego elimina la cabecera
                    string sqlPedido = "DELETE FROM Pedidos WHERE IdPedido = @IdPedido";
                    using (SqlCommand cmd = new SqlCommand(sqlPedido, conexion, transaccion))
                    {
                        cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                        cmd.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    throw new Exception($"Error al eliminar pedido: {ex.Message}");
                }
            }
        }
    }
}