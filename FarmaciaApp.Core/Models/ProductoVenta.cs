/**
 * @file ProductoVenta.cs
 * @brief Producto con el precio que se cobra al vender.
 * @author Santiago Caicedo
 */
using System;
using FarmaciaApp.Core;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Producto con su precio del día, aplicando la mejor promoción vigente.
     */
    public class ProductoVenta
    {
        /** Identificador del producto. */
        public int ProId { get; set; }
        /** Nombre. */
        public string ProNombre { get; set; }
        /** Precio sin descuento. */
        public decimal PrecioBase { get; set; }
        /** Porcentaje de descuento de la promoción vigente (0 si no tiene). */
        public decimal Descuento { get; set; }
        /** Unidades disponibles. */
        public int Stock { get; set; }

        /** Precio con el descuento aplicado, redondeado a 2 decimales. */
        public decimal PrecioFinal => Math.Round(PrecioBase * (1 - Descuento / 100m), 2);

        /**
         * @brief Texto que se muestra en la lista de productos de una venta.
         * @return Nombre, precio, descuento y stock
         */
        public override string ToString() =>
            Descuento > 0
                ? $"{ProNombre}  ·  {Formato.Numero(PrecioFinal)} (-{Descuento:0.#}%)  ·  stock {Stock}"
                : $"{ProNombre}  ·  {Formato.Numero(PrecioFinal)}  ·  stock {Stock}";
    }
}
