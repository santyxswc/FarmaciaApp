/**
 * @file Persona.cs
 * @brief Modelo de persona.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Datos personales de clientes, vendedores y empleados (TBL_PERSONA).
     *
     * Una persona puede ser cliente, vendedor, ambos o ninguno.
     */
    public class Persona
    {
        /** Identificador (PER_ID). */
        public int PerId { get; set; }
        /** Nombre. */
        public string PerNombre { get; set; }
        /** Apellido. */
        public string PerApellido { get; set; }
        /** Dirección. */
        public string PerDireccion { get; set; }
        /** Teléfono. */
        public string PerTelefono { get; set; }
        /** Correo electrónico. */
        public string PerEmail { get; set; }

        /** Indica si esta registrada en TBL_CLIENTE. */
        public bool EsCliente { get; set; }
        /** Indica si esta registrada en TBL_VENDEDOR. */
        public bool EsVendedor { get; set; }

        /** Nombre y apellido. */
        public string NombreCompleto => $"{PerNombre} {PerApellido}";

        /** Roles como texto para mostrar en la lista ("Cliente, Vendedor"). */
        public string Roles =>
            EsCliente && EsVendedor ? "Cliente, Vendedor"
            : EsCliente ? "Cliente"
            : EsVendedor ? "Vendedor"
            : "—";
    }
}
