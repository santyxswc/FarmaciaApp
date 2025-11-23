using System;
using System.Collections.Generic;
using System.Text;


namespace FarmaciaApp.Core.Models
{
    public class Promocion
    {
        public int PrmId { get; set; }
        public string PrmDescripcion { get; set; }
        public decimal PrmDescuento { get; set; }
        public DateTime PrmFechaIni { get; set; }
        public DateTime PrmFechaFin { get; set; }

        // Propiedad calculada para UI
        public string Vigencia => $"{PrmFechaIni:dd/MM/yyyy} - {PrmFechaFin:dd/MM/yyyy}";
        public bool EstaActiva => DateTime.Now >= PrmFechaIni && DateTime.Now <= PrmFechaFin;
    }
}
