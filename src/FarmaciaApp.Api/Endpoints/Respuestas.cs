/**
 * @file Respuestas.cs
 * @brief Respuestas HTTP comunes a los endpoints.
 * @author Santiago Caicedo
 */
namespace FarmaciaApp.Api.Endpoints
{
    /**
     * @brief Atajos para las respuestas que se repiten en todos los módulos.
     */
    public static class Respuestas
    {
        /**
         * @brief Respuesta 404 en formato ProblemDetails.
         * @param recurso Nombre del recurso, para el título ("Producto")
         * @return 404
         */
        public static IResult NoEncontrado(string recurso) =>
            Results.Problem(statusCode: StatusCodes.Status404NotFound, title: $"{recurso} no encontrado");

        /**
         * @brief Traduce el resultado booleano de un servicio.
         * @param actualizado Lo que devolvió el servicio
         * @param recurso Nombre del recurso, para el 404
         * @return 204 si se aplicó el cambio; 404 si no
         */
        public static IResult SinContenidoOSiNoExiste(bool actualizado, string recurso) =>
            actualizado ? Results.NoContent() : NoEncontrado(recurso);

        /**
         * @brief Convierte el periodo de una consulta a fechas.
         * @param desde Fecha inicial; por defecto, hace 6 días
         * @param hasta Fecha final; por defecto, hoy
         * @return Fechas para los servicios de Core
         */
        public static (DateTime Desde, DateTime Hasta) Periodo(DateOnly? desde, DateOnly? hasta)
        {
            var fin = hasta ?? DateOnly.FromDateTime(DateTime.Now);
            var inicio = desde ?? fin.AddDays(-6);
            return (inicio.ToDateTime(TimeOnly.MinValue), fin.ToDateTime(TimeOnly.MinValue));
        }
    }
}
