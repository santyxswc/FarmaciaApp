/**
 * @file Movimiento.cs
 * @brief Modelo de movimiento del registro de actividad.
 * @author Santiago Caicedo
 */
using System;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Acción registrada en TBL_MOVIMIENTO: quien hizo que y cuando.
     */
    public class Movimiento
    {
        /** Identificador del movimiento. */
        public decimal MovId { get; set; }
        /** Fecha y hora. */
        public DateTime Fecha { get; set; }
        /** Login del usuario, o "(sin sesión)" en intentos de ingreso fallidos. */
        public string Usuario { get; set; }
        /** Rol del usuario. */
        public string Rol { get; set; }
        /** Acción realizada ("Venta registrada", "Producto modificado"...). */
        public string Accion { get; set; }
        /** Detalle de la acción. */
        public string Detalle { get; set; }
    }
}
