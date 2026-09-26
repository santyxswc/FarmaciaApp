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

        // Roles: la persona puede ser cliente, vendedor, ambos o ninguno
        public bool EsCliente { get; set; }
        public bool EsVendedor { get; set; }

        public string NombreCompleto => $"{PerNombre} {PerApellido}";

        public string Roles =>
            EsCliente && EsVendedor ? "Cliente, Vendedor"
            : EsCliente ? "Cliente"
            : EsVendedor ? "Vendedor"
            : "—";
    }
}
