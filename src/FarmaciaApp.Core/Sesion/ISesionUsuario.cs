/**
 * @file ISesionUsuario.cs
 * @brief Contrato de la sesión del usuario del turno.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Sesion
{
    /**
     * @brief Usuario autenticado y reglas de acceso.
     */
    public interface ISesionUsuario
    {
        /** Usuario del turno actual; null si no hay sesión. */
        Usuario Usuario { get; }
        /** VEN_ID del usuario si es vendedor; sus ventas quedan a su nombre. */
        decimal? VenId { get; }
        /** Momento en que se inició la sesión. */
        DateTime InicioTurno { get; }
        /** Indica si hay una sesión iniciada. */
        bool Activa { get; }
        /** Indica si el usuario del turno es administrador. */
        bool EsAdmin { get; }

        /**
         * @brief Abre la sesión de un usuario.
         * @param usuario Usuario autenticado
         * @param venId VEN_ID asociado a su persona, o null
         */
        void Iniciar(Usuario usuario, decimal? venId);

        /** @brief Cierra la sesión actual. */
        void Cerrar();

        /**
         * @brief Verifica que haya una sesión iniciada.
         * @exception UnauthorizedAccessException Si no hay sesión
         */
        void ExigirSesion();

        /**
         * @brief Verifica que el usuario del turno sea administrador.
         * @param accion Acción que se intenta, para el mensaje de error ("crear productos")
         * @exception UnauthorizedAccessException Si no hay sesión o el usuario no es administrador
         */
        void ExigirAdmin(string accion);
    }
}
