using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Models
{
    public class Factura
    {
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
        public List<FacturaProductoDetalle> Items { get; set; }
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