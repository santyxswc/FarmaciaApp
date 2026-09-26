/**
 * @file Auditoria.cs
 * @brief Registro de movimientos de los usuarios.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Repositories;
using System;
using System.Diagnostics;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Guarda en TBL_MOVIMIENTO lo que hace el usuario de la sesión actual.
     *
     * Los servicios la llaman después de cada operación exitosa. Si el registro falla, el error se escribe
     * en la traza y no se propaga, porque la operación principal ya quedó guardada.
     */
    public static class Auditoria
    {
        /** Largo máximo del detalle (columna MOV_DETALLE). */
        private const int LargoMaximoDetalle = 500;

        /**
         * @brief Registra un movimiento a nombre del usuario de la sesión.
         * @param accion Acción realizada
         * @param detalle Detalle; se recorta si supera 500 caracteres
         */
        public static void Registrar(string accion, string detalle = null)
        {
            if (detalle != null && detalle.Length > LargoMaximoDetalle)
                detalle = detalle.Substring(0, LargoMaximoDetalle - 3) + "...";

            try
            {
                new MovimientoRepository().Insert(Sesion.Usuario?.UsuId, accion, detalle);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"No se pudo registrar el movimiento '{accion}': {ex.Message}");
            }
        }
    }
}
