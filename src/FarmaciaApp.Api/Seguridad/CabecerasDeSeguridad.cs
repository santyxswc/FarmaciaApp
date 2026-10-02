/**
 * @file CabecerasDeSeguridad.cs
 * @brief Cabeceras HTTP de seguridad para la interfaz web.
 * @author Santiago Caicedo
 */
namespace FarmaciaApp.Api.Seguridad
{
    /**
     * @brief Agrega las cabeceras que endurecen el navegador frente a XSS y clickjacking.
     *
     * La política de contenido solo permite recursos del propio origen. La documentación
     * interactiva (/scalar) usa scripts y estilos propios, por eso queda fuera de esa política.
     */
    public sealed class CabecerasDeSeguridad
    {
        /** Política de contenido de la interfaz web. */
        private const string PoliticaContenido =
            "default-src 'self'; img-src 'self' data:; frame-ancestors 'none'; base-uri 'none'; form-action 'self'";

        /** Siguiente paso de la tubería. */
        private readonly RequestDelegate _siguiente;

        /**
         * @brief Crea el middleware.
         * @param siguiente Siguiente paso de la tubería
         */
        public CabecerasDeSeguridad(RequestDelegate siguiente) => _siguiente = siguiente;

        /**
         * @brief Agrega las cabeceras a la respuesta.
         * @param contexto Petición actual
         * @return Tarea de la petición
         */
        public Task InvokeAsync(HttpContext contexto)
        {
            var cabeceras = contexto.Response.Headers;
            cabeceras["X-Content-Type-Options"] = "nosniff";
            cabeceras["Referrer-Policy"] = "no-referrer";
            if (!contexto.Request.Path.StartsWithSegments("/scalar"))
                cabeceras["Content-Security-Policy"] = PoliticaContenido;
            return _siguiente(contexto);
        }
    }
}
