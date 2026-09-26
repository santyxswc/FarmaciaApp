/**
 * @file Reclamo.cs
 * @brief Modelo de reclamo.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Reclamo de un cliente sobre una factura (TBL_RECLAMO).
     */
    public class Reclamo
    {
        /** Número del reclamo. */
        public decimal RecId { get; set; }
        /** Fecha de registro. */
        public DateTime RecFecha { get; set; }
        /** Descripción del problema. */
        public string RecDescripcion { get; set; }
        /** Estado: Pendiente, En proceso, Resuelto o Rechazado. */
        public string RecEstado { get; set; }
        /** Factura reclamada. */
        public decimal FacNumFactura { get; set; }
        /** Nombre del cliente de la factura. */
        public string ClienteNombre { get; set; }
    }
}
