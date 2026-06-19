using System;
using System.Security.Cryptography;
using System.Text;

namespace App.BLL.Servicios
{
    /// Clase utilitaria para encriptar contraseñas con SHA256.
    /// SHA256 es un algoritmo de hash — convierte cualquier texto en
    /// una cadena fija de 64 caracteres hexadecimales.
    /// El hash NO se puede revertir — por eso es seguro para contraseñas.
    public static class Encriptacion
    {
        /// Convierte una contraseña en texto plano a su hash SHA256.
        /// Ejemplo: "admin123" → "a665a45920422f9d417e4867efdc4fb8..."
        public static string EncriptarSHA256(string texto)
        {
            // SHA256.Create() instancia el algoritmo de hash
            using (SHA256 sha256 = SHA256.Create())
            {
                // Encoding.UTF8.GetBytes: convierte el string a arreglo de bytes
                // porque SHA256 trabaja con bytes, no con strings
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));

                // StringBuilder construye el string hexadecimal resultado
                StringBuilder resultado = new StringBuilder();

                // Convierte cada byte a su representación hexadecimal de 2 caracteres
                // "x2" significa: hexadecimal en minúscula, mínimo 2 dígitos
                foreach (byte b in bytes)
                    resultado.Append(b.ToString("x2"));

                return resultado.ToString();
            }
        }
    }
}