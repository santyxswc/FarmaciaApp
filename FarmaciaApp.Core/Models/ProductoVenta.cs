using System;

namespace FarmaciaApp.Core.Models
{
    // Producto con el precio que se cobra hoy (aplicando la mejor promocion activa)
    public class ProductoVenta
    {
        public int ProId { get; set; }
        public string ProNombre { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal Descuento { get; set; }
        public int Stock { get; set; }

        public decimal PrecioFinal => Math.Round(PrecioBase * (1 - Descuento / 100m), 2);

        public override string ToString() =>
            Descuento > 0
                ? $"{ProNombre}  ·  {PrecioFinal:N0} (-{Descuento:0.#}%)  ·  stock {Stock}"
                : $"{ProNombre}  ·  {PrecioFinal:N0}  ·  stock {Stock}";
    }
}
