/**
 * @file Seleccion.cs
 * @brief Opción de una lista desplegable.
 * @author Santiago Caicedo
 */
namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Par identificador y nombre para listas desplegables (cliente, vendedor, factura...).
     */
    public class Seleccion
    {
        /** Identificador del registro. */
        public decimal Id { get; set; }
        /** Texto que se muestra. */
        public string Nombre { get; set; }

        /**
         * @brief Texto que muestra la lista desplegable.
         * @return El nombre
         */
        public override string ToString() => Nombre;
    }
}
