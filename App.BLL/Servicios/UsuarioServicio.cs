using App.DAL.Modelos;
using App.DAL.Repositorios;
using System;
using System.Collections.Generic;

namespace App.BLL.Servicios
{
    /// <summary>
    /// Servicio de Usuarios: autenticación y gestión completa.
    /// Encripta contraseñas antes de enviarlas a DAL.
    /// </summary>
    public class UsuarioServicio
    {
        private readonly UsuarioRepositorio _repositorio;

        public UsuarioServicio()
        {
            _repositorio = new UsuarioRepositorio();
        }

        /// <summary>
        /// Autentica: encripta la contraseña y consulta DAL.
        /// </summary>
        public Usuario? Autenticar(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
                return null;

            string hash = Encriptacion.EncriptarSHA256(contrasena);
            return _repositorio.Autenticar(nombreUsuario, hash);
        }

        public List<Usuario> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Perfil> ObtenerPerfiles()
        {
            return _repositorio.ObtenerPerfiles();
        }

        public string Insertar(Usuario usuario, string contrasenaPlana)
        {
            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
                return "El nombre de usuario es obligatorio.";

            if (usuario.NombreUsuario.Length < 4)
                return "El nombre de usuario debe tener mínimo 4 caracteres.";

            if (string.IsNullOrWhiteSpace(contrasenaPlana))
                return "La contraseña es obligatoria.";

            if (contrasenaPlana.Length < 6)
                return "La contraseña debe tener mínimo 6 caracteres.";

            if (usuario.IdPerfil <= 0)
                return "Debe seleccionar un perfil.";

            // Encripta la contraseña antes de guardarla — nunca texto plano en BD
            usuario.Contrasena = Encriptacion.EncriptarSHA256(contrasenaPlana);

            try
            {
                bool resultado = _repositorio.Insertar(usuario);
                return resultado ? null : "No se pudo guardar el usuario.";
            }
            catch (Exception ex)
            {
                return $"Error al guardar: {ex.Message}";
            }
        }

        public string Actualizar(Usuario usuario, string contrasenaPlana)
        {
            if (usuario.Id <= 0)
                return "Debe seleccionar un usuario para editar.";

            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
                return "El nombre de usuario es obligatorio.";

            if (usuario.IdPerfil <= 0)
                return "Debe seleccionar un perfil.";

            // Si escribió nueva contraseña la encripta, si no la deja igual
            bool cambiarContrasena = !string.IsNullOrWhiteSpace(contrasenaPlana);

            if (cambiarContrasena)
            {
                if (contrasenaPlana.Length < 6)
                    return "La contraseña debe tener mínimo 6 caracteres.";

                usuario.Contrasena = Encriptacion.EncriptarSHA256(contrasenaPlana);
            }

            try
            {
                bool resultado = _repositorio.Actualizar(usuario, cambiarContrasena);
                return resultado ? null : "No se pudo actualizar el usuario.";
            }
            catch (Exception ex)
            {
                return $"Error al actualizar: {ex.Message}";
            }
        }

        public string Eliminar(int idUsuario, int idUsuarioActual)
        {
            if (idUsuario <= 0)
                return "ID de usuario inválido.";

            // Regla de negocio: no puedes eliminarte a ti mismo
            if (idUsuario == idUsuarioActual)
                return "No puede eliminar el usuario con el que está logueado.";

            try
            {
                bool resultado = _repositorio.Eliminar(idUsuario);
                return resultado ? null : "No se encontró el usuario o ya fue eliminado.";
            }
            catch (Exception ex)
            {
                return $"Error al eliminar: {ex.Message}";
            }
        }
    }
}