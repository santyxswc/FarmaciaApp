/**
 * @file Proveedor.cs
 * @brief Modelo de proveedor.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Laboratorio o distribuidor (TBL_PROVEEDOR).
     */
    public class Proveedor
    {
        /** Identificador. */
        public int ProId { get; set; }
        /** Nombre de la empresa. */
        public string ProNombre { get; set; }
        /** Persona de contacto. */
        public string ProContacto { get; set; }
        /** Teléfono. */
        public string ProTelefono { get; set; }
    }
}
