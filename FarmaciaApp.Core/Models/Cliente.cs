using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    // ¡IMPORTANTE! Debe ser 'public' para que el proyecto UI la pueda usar.
    public class Cliente
    {
        // 1. Campos heredados de TBL_PERSONA (según tu script)
        public decimal PerId { get; set; }        // ID
        public string PerNombre { get; set; }     // Nombre
        public string PerApellido { get; set; }   // Apellido
        public string PerDireccion { get; set; }  // Dirección
        public string PerTelefono { get; set; }   // Teléfono
        public string PerEmail { get; set; }      // Email

        // 2. Otros campos específicos de TBL_CLIENTE (Si los hay. Si TBL_CLIENTE solo tiene la FK a PERSONA, con los campos anteriores basta)
        // Si TBL_CLIENTE tiene campos extra, añádelos aquí.
        // Ejemplo: public string CliEstado { get; set; } 
    }
}
