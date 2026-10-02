/**
 * @file ManejadorErrores.cs
 * @brief Traduce las excepciones de Core a respuestas HTTP.
 * @author Santiago Caicedo
 */
using System.Data.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FarmaciaApp.Api.Errores
{
    /**
     * @brief Convierte las excepciones de las reglas de negocio en ProblemDetails (RFC 9457).
     *
     * ArgumentException es 400, UnauthorizedAccessException es 403 e InvalidOperationException es 409.
     * Un fallo de la base de datos es 503. Cualquier otra excepción es 500 y no expone detalles al cliente.
     */
    public sealed class ManejadorErrores : IExceptionHandler
    {
        /** Registro de errores inesperados. */
        private readonly ILogger<ManejadorErrores> _log;
        /** Escritor de respuestas ProblemDetails. */
        private readonly IProblemDetailsService _problemas;

        /**
         * @brief Crea el manejador.
         * @param log Registro de errores
         * @param problemas Escritor de ProblemDetails
         */
        public ManejadorErrores(ILogger<ManejadorErrores> log, IProblemDetailsService problemas)
        {
            _log = log;
            _problemas = problemas;
        }

        /**
         * @brief Escribe la respuesta de error.
         * @param contexto Petición que falló
         * @param excepcion Excepción lanzada
         * @param cancelacion Token de cancelación
         * @return true, siempre: la excepción queda atendida
         */
        public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken cancelacion)
        {
            var (estado, titulo, detalle) = excepcion switch
            {
                ArgumentException e => (StatusCodes.Status400BadRequest, "Datos inválidos", e.Message),
                UnauthorizedAccessException e => (StatusCodes.Status403Forbidden, "Acción no permitida", e.Message),
                DbException => (StatusCodes.Status503ServiceUnavailable, "Base de datos no disponible", "No se pudo consultar la base de datos. Intenta de nuevo en un momento."),
                InvalidOperationException e => (StatusCodes.Status409Conflict, "Operación no válida", e.Message),
                _ => (StatusCodes.Status500InternalServerError, "Error del servidor", "Ocurrió un error inesperado.")
            };

            if (estado >= StatusCodes.Status500InternalServerError)
                _log.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);

            contexto.Response.StatusCode = estado;
            return await _problemas.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = contexto,
                Exception = excepcion,
                ProblemDetails = new ProblemDetails { Status = estado, Title = titulo, Detail = detalle }
            });
        }
    }
}
