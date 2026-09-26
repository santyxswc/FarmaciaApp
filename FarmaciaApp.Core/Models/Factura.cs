using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Models
{
    public class Factura
    {
        // Los precios de los productos ya incluyen el IVA
        public const decimal TasaIva = 0.19m;

        public decimal FacNumFactura { get; set; }
        public DateTime FacFecha { get; set; }
        public decimal FacSubtotal { get; set; }
        public decimal FacIva { get; set; }
        public decimal FacTotal { get; set; }
        public decimal CliId { get; set; }
        public decimal VenId { get; set; }
        public decimal PagId { get; set; }
        public string ClienteNombre { get; set; } 
        public string VendedorNombre { get; set; }
        public string MetodoPago { get; set; }
        public List<FacturaProductoDetalle> Items { get; set; }

        // Separa el IVA de un total que ya lo incluye: subtotal = total / 1.19
        public static (decimal Subtotal, decimal Iva) DesglosarIva(decimal total)
        {
            decimal subtotal = Math.Round(total / (1 + TasaIva), 2);
            return (subtotal, total - subtotal);
        }
    }
    public class FacturaProductoDetalle
    {
        public decimal ProId { get; set; }
        public string ProNombre { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }
    }
}
