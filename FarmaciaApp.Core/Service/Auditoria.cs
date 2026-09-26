using FarmaciaApp.Core.Repositories;
using System;
using System.Diagnostics;

namespace FarmaciaApp.Core.Services
{
    // Registra en TBL_MOVIMIENTO lo que hace el usuario de la sesion actual
    public static class Auditoria
    {
        private const int LargoMaximoDetalle = 500;

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
                // Si el registro falla, la operacion principal (ya guardada) no debe mostrarse como fallida
                Trace.WriteLine($"No se pudo registrar el movimiento '{accion}': {ex.Message}");
            }
        }
    }
}
