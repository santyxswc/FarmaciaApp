/**
 * @file Sesion.cs
 * @brief Sesión del usuario que tiene abierto el turno.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using System;

namespace FarmaciaApp.Core
{
    /**
     * @brief Usuario autenticado en la aplicación y reglas de acceso.
     *
     * La inician y la cierran UsuarioService.IniciarSesion y UsuarioService.CerrarSesion.
     * Los servicios la consultan para validar permisos y para registrar movimientos.
     */
    public static class Sesion
    {
        /** Usuario del turno actual; null si no hay sesión. */
        public static Usuario Usuario { get; private set; }

        /** VEN_ID del usuario si es vendedor; sus ventas quedan a su nombre. */
        public static decimal? VenId { get; private set; }

        /** Momento en que se inició la sesión. */
        public static DateTime InicioTurno { get; private set; }

        /** Indica si hay una sesión iniciada. */
        public static bool Activa => Usuario != null;
        /** Indica si el usuario del turno es administrador. */
        public static bool EsAdmin => Usuario?.EsAdmin == true;

        /**
         * @brief Abre la sesión de un usuario.
         * @param usuario Usuario autenticado
         * @param venId VEN_ID asociado a su persona, o null
         */
        public static void Iniciar(Usuario usuario, decimal? venId)
        {
            Usuario = usuario;
            VenId = venId;
            InicioTurno = DateTime.Now;
        }

        /**
         * @brief Cierra la sesión actual.
         */
        public static void Cerrar()
        {
            Usuario = null;
            VenId = null;
        }

        /**
         * @brief Verifica que haya una sesión iniciada.
         * @exception UnauthorizedAccessException Si no hay sesión
         */
        public static void ExigirSesion()
        {
            if (!Activa)
                throw new UnauthorizedAccessException("Debes iniciar sesión para realizar esta operación.");
        }

        /**
         * @brief Verifica que el usuario del turno sea administrador.
         * @param accion Acción que se intenta, para el mensaje de error ("crear productos")
         * @exception UnauthorizedAccessException Si no hay sesión o el usuario no es administrador
         */
        public static void ExigirAdmin(string accion)
        {
            ExigirSesion();
            if (!EsAdmin)
                throw new UnauthorizedAccessException($"Solo el administrador puede {accion}.");
        }
    }
}
