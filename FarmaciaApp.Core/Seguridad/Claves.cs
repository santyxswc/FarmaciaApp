using System;
using System.Security.Cryptography;

namespace FarmaciaApp.Core.Seguridad
{
    // Contraseñas con PBKDF2-SHA256 y sal aleatoria: en la base solo queda el hash
    public static class Claves
    {
        private const int Iteraciones = 100_000;
        private const int BytesHash = 32;

        public static (string Hash, string Sal) Crear(string clave)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(16);
            return (Convert.ToBase64String(Calcular(clave, sal)), Convert.ToBase64String(sal));
        }

        public static bool Verificar(string clave, string hash, string sal)
        {
            if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(sal))
                return false;

            byte[] esperado = Convert.FromBase64String(hash);
            byte[] calculado = Calcular(clave, Convert.FromBase64String(sal));
            return CryptographicOperations.FixedTimeEquals(esperado, calculado);
        }

        private static byte[] Calcular(string clave, byte[] sal) =>
            Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);
    }
}
