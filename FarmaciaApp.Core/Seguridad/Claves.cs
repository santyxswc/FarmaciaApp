/**
 * @file Claves.cs
 * @brief Hash y verificación de contraseñas.
 * @author Santiago Caicedo
 */
using System;
using System.Security.Cryptography;

namespace FarmaciaApp.Core.Seguridad
{
    /**
     * @brief Contrasenas con PBKDF2-SHA256 y sal aleatoria.
     *
     * En la base de datos solo se guardan el hash y la sal en Base64.
     */
    public static class Claves
    {
        /** Iteraciones de PBKDF2. */
        private const int Iteraciones = 100_000;
        /** Longitud del hash en bytes. */
        private const int BytesHash = 32;

        /**
         * @brief Genera el hash de una contraseña nueva.
         * @param clave Contraseña en texto plano
         * @return Hash y sal en Base64
         */
        public static (string Hash, string Sal) Crear(string clave)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(16);
            return (Convert.ToBase64String(Calcular(clave, sal)), Convert.ToBase64String(sal));
        }

        /**
         * @brief Comprueba una contraseña contra su hash guardado.
         * @param clave Contraseña escrita por el usuario
         * @param hash Hash guardado (Base64)
         * @param sal Sal guardada (Base64)
         * @return true si la contraseña es correcta
         *
         * La comparación toma el mismo tiempo sin importar donde difieran los bytes.
         */
        public static bool Verificar(string clave, string hash, string sal)
        {
            if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(sal))
                return false;

            byte[] esperado = Convert.FromBase64String(hash);
            byte[] calculado = Calcular(clave, Convert.FromBase64String(sal));
            return CryptographicOperations.FixedTimeEquals(esperado, calculado);
        }

        /**
         * @brief Calcula el hash PBKDF2 de una contraseña.
         * @param clave Contraseña en texto plano
         * @param sal Sal en bytes
         * @return Hash en bytes
         */
        private static byte[] Calcular(string clave, byte[] sal) =>
            Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);
    }
}
