using System;

namespace FarmaciaApp.Core.Models
{
    // Accion registrada en TBL_MOVIMIENTO (quien hizo que y cuando)
    public class Movimiento
    {
        public decimal MovId { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Rol { get; set; }
        public string Accion { get; set; }
        public string Detalle { get; set; }
    }
}
