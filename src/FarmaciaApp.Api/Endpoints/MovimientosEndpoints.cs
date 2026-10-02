/**
 * @file MovimientosEndpoints.cs
 * @brief Endpoint del registro de movimientos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Acción registrada de un usuario. */
    public sealed record MovimientoDto(decimal Id, DateTime Fecha, string Usuario, string Rol, string Accion, string Detalle);

    /**
     * @brief Auditoría de lo que hace cada usuario. Solo administrador.
     */
    public static class MovimientosEndpoints
    {
        /**
         * @brief Registra la ruta /api/movimientos.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapMovimientos(this IEndpointRouteBuilder rutas)
        {
            rutas.MapGet("/api/movimientos", (MovimientoService movimientos, DateOnly? desde, DateOnly? hasta, decimal? usuarioId, string buscar) =>
                {
                    var (ini, fin) = Respuestas.Periodo(desde, hasta);
                    return Results.Ok(movimientos.Buscar(ini, fin, usuarioId, buscar)
                        .Select(m => new MovimientoDto(m.MovId, m.Fecha, m.Usuario, m.Rol, m.Accion, m.Detalle)));
                })
                .WithTags("Movimientos")
                .RequireAuthorization()
                .WithSummary("Movimientos filtrados por periodo, usuario y texto; hasta 500 (administrador)")
                .Produces<IEnumerable<MovimientoDto>>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            return rutas;
        }
    }
}
