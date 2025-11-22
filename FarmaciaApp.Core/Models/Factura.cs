using System;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Models
{
    public class Factura
    {
        // Campos de TBL_FACTURA
        public decimal FacNumFactura { get; set; } // PK (NUMBER)
        public DateTime FacFecha { get; set; }
        public decimal FacSubtotal { get; set; } // NUMBER(12,2)
        public decimal FacIva { get; set; }      // NUMBER(12,2)
        public decimal FacTotal { get; set; }    // NUMBER(12,2)
        
        // Relaciones (Foreign Keys)
        public decimal CliId { get; set; }       // FK a TBL_CLIENTE
        public decimal VenId { get; set; }       // FK a TBL_VENDEDOR
        public decimal PagId { get; set; }       // FK a TBL_PAGO

        // Propiedad de navegación (para mostrar detalles en la UI sin hacer joins complejos)
        public string ClienteNombre { get; set; } 
        public string VendedorNombre { get; set; } 

        // Lista de ítems facturados (para la vista de detalle)
        public List<FacturaProductoDetalle> Items { get; set; }
    }

    // Modelo para los detalles de la línea de la factura (FACTU_PRODUC)
    public class FacturaProductoDetalle
    {
        public decimal ProId { get; set; }
        public string ProNombre { get; set; } // Nombre del producto (traído por join)
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; } // Cantidad * Precio
    }
}