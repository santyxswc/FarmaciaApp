/**
 * @file ServicioAuditoria.cs
 * @brief Registro de movimientos de los usuarios.
 * @author Santiago Caicedo
 */
using System;
using System.Diagnostics;
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Guarda en TBL_MOVIMIENTO lo que hace el usuario de la sesión actual.
     *
     * Los servicios la llaman después de cada operación exitosa. Si el registro falla, el error se escribe
     * en la traza y no se propaga, porque la operación principal ya quedó guardada.
     */
    public sealed class ServicioAuditoria : IAuditoria
    {
        /** Movimientos en la base de datos. */
        private readonly IMovimientoRepository _movimientos;
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;
        /** Largo máximo del detalle (columna MOV_DETALLE). */
        private const int LargoMaximoDetalle = 500;

        /**
         * @brief Crea el servicio.
         * @param movimientos Repositorio de movimientos
         * @param sesion Usuario del turno
         */
        public ServicioAuditoria(IMovimientoRepository movimientos, ISesionUsuario sesion)
        {
            _movimientos = movimientos;
            _sesion = sesion;
        }

        /**
         * @brief Registra un movimiento a nombre del usuario de la sesión.
         * @param accion Acción realizada
         * @param detalle Detalle; se recorta si supera 500 caracteres
         */
        public void Registrar(string accion, string detalle = null)
        {
            if (detalle != null && detalle.Length > LargoMaximoDetalle)
                detalle = detalle.Substring(0, LargoMaximoDetalle - 3) + "...";

            try
            {
                _movimientos.Insert(_sesion.Usuario?.UsuId, accion, detalle);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"No se pudo registrar el movimiento '{accion}': {ex.Message}");
            }
        }
    }
}
