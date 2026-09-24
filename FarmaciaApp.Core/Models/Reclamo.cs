using System;
using System.Collections.Generic;
using System.Text;


namespace FarmaciaApp.Core.Models
{
    public class Reclamo
    {
        public decimal RecId { get; set; }
        public DateTime RecFecha { get; set; }
        public string RecDescripcion { get; set; }
        public string RecEstado { get; set; }
        public decimal FacNumFactura { get; set; }
        public string ClienteNombre { get; set; }
    }
}
