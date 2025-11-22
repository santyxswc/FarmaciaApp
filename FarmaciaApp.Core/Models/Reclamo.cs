using System;
using System.Collections.Generic;
using System.Text;


namespace FarmaciaApp.Core.Models
{
    public class Reclamo
    {
        // Campos de TBL_RECLAMO
        public decimal RecId { get; set; }         // PK
        public DateTime RecFecha { get; set; }
        public string RecDescripcion { get; set; }
        public string RecEstado { get; set; }     // Ejemplo: 'Pendiente', 'Resuelto', 'Rechazado'

        // Relación
        public decimal FacNumFactura { get; set; } // FK a TBL_FACTURA

        // Propiedades de ayuda para la UI (obtenidas por join)
        public string ClienteNombre { get; set; } // Nombre del cliente que hizo la factura
    }
}
