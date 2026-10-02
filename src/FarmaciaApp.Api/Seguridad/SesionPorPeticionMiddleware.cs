/**
 * @file SesionPorPeticionMiddleware.cs
 * @brief Abre la sesión de Core con el usuario del token.
 * @author Santiago Caicedo
 */
using System.Globalization;
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Api.Seguridad
{
    /**
     * @brief Convierte el token validado en la ISesionUsuario de la petición.
     *
     * Los servicios de Core verifican permisos con la sesión; la API la crea de nuevo en cada
     * petición a partir de la base de datos. Si la cuenta ya no existe o fue desactivada, responde 401.
     */
    public sealed class SesionPorPeticionMiddleware
    {
        /** Siguiente paso de la tubería. */
        private readonly RequestDelegate _siguiente;

        /**
         * @brief Crea el middleware.
         * @param siguiente Siguiente paso de la tubería
         */
        public SesionPorPeticionMiddleware(RequestDelegate siguiente) => _siguiente = siguiente;

        /**
         * @brief Abre la sesión si la petición trae un token válido.
         * @param contexto Petición actual
         * @param usuarios Acceso a datos de usuarios
         * @param sesion Sesión de la petición
         */
        public async Task InvokeAsync(HttpContext contexto, IUsuarioRepository usuarios, ISesionUsuario sesion)
        {
            if (contexto.User.Identity?.IsAuthenticated == true)
            {
                var sub = contexto.User.FindFirst("sub")?.Value;
                var usuario = decimal.TryParse(sub, NumberStyles.Number, CultureInfo.InvariantCulture, out var id)
                    ? usuarios.GetById(id)
                    : null;

                if (usuario == null || !usuario.Activo)
                {
                    contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                decimal? venId = usuario.PerId.HasValue ? usuarios.GetVenIdPorPersona(usuario.PerId.Value) : null;
                sesion.Iniciar(usuario, venId);
            }

            await _siguiente(contexto);
        }
    }
}
