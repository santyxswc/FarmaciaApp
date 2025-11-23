using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{

    public class Cliente
    {
        // 1. Campos heredados de TBL_PERSONA
        public decimal PerId { get; set; }        // ID
        public string PerNombre { get; set; }     // Nombre
        public string PerApellido { get; set; }   // Apellido
        public string PerDireccion { get; set; }  // Dirección
        public string PerTelefono { get; set; }   // Teléfono
        public string PerEmail { get; set; }      // Email

        
    }
}
