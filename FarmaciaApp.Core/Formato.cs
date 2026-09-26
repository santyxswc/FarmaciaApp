/**
 * @file Formato.cs
 * @brief Formato de moneda y números de la aplicación.
 * @author Santiago Caicedo
 */
using System.Globalization;

namespace FarmaciaApp.Core
{
    /**
     * @brief Formatea valores con la cultura es-CO (pesos colombianos).
     *
     * Se usa en los textos que se guardan en la base de datos, para que no dependan
     * del idioma del sistema operativo.
     */
    public static class Formato
    {
        /** Cultura de la aplicación: español de Colombia. */
        public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CO");

        /**
         * @brief Formatea un valor como moneda sin decimales.
         * @param valor Valor en pesos
         * @return Texto como "$ 8.500"
         */
        public static string Moneda(decimal valor) => valor.ToString("C0", Cultura);

        /**
         * @brief Formatea un número con separador de miles.
         * @param valor Valor a formatear
         * @return Texto como "8.500"
         */
        public static string Numero(decimal valor) => valor.ToString("N0", Cultura);
    }
}
