/**
 * @file Promocion.cs
 * @brief Modelo de promoción.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Descuento con fechas de vigencia (TBL_PROMOCION).
     */
    public class Promocion
    {
        /** Identificador (PRM_ID). */
        public int PrmId { get; set; }
        /** Descripción. */
        public string PrmDescripcion { get; set; }
        /** Porcentaje de descuento. */
        public decimal PrmDescuento { get; set; }
        /** Fecha de inicio. */
        public DateTime PrmFechaIni { get; set; }
        /** Fecha de fin. */
        public DateTime PrmFechaFin { get; set; }

        /** Periodo de vigencia como texto. */
        public string Vigencia => $"{PrmFechaIni:dd/MM/yyyy} - {PrmFechaFin:dd/MM/yyyy}";
        /** Indica si hoy está dentro de la vigencia. */
        public bool EstaActiva => DateTime.Now >= PrmFechaIni && DateTime.Now <= PrmFechaFin;
    }
}
