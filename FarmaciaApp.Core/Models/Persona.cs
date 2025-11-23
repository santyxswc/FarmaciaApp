using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    public class Persona
    {
        public int PerId { get; set; }
        public string PerNombre { get; set; }
        public string PerApellido { get; set; }
        public string PerDireccion { get; set; }
        public string PerTelefono { get; set; }
        public string PerEmail { get; set; }

        // Propiedad calculada para mostrar en UI
        public string NombreCompleto => $"{PerNombre} {PerApellido}";
    }
}
