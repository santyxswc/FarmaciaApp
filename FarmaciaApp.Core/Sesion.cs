using FarmaciaApp.Core.Models;
using System;

namespace FarmaciaApp.Core
{
    // Usuario que tiene abierto el turno en esta aplicacion
    public static class Sesion
    {
        public static Usuario Usuario { get; private set; }

        // VEN_ID del empleado (las ventas quedan a su nombre); null si no es vendedor
        public static decimal? VenId { get; private set; }

        public static DateTime InicioTurno { get; private set; }

        public static bool Activa => Usuario != null;
        public static bool EsAdmin => Usuario?.EsAdmin == true;

        public static void Iniciar(Usuario usuario, decimal? venId)
        {
            Usuario = usuario;
            VenId = venId;
            InicioTurno = DateTime.Now;
        }

        public static void Cerrar()
        {
            Usuario = null;
            VenId = null;
        }

        // Sin sesion (version WPF, que no tiene usuarios) no se aplican restricciones
        public static void ExigirAdmin(string accion)
        {
            if (Activa && !EsAdmin)
                throw new UnauthorizedAccessException($"Solo el administrador puede {accion}.");
        }
    }
}
