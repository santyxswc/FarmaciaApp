/**
 * @file PromocionesEndpoints.cs
 * @brief Endpoints de promociones.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Promoción con su vigencia. */
    public sealed record PromocionDto(int Id, string Descripcion, decimal Descuento, DateTime FechaInicio, DateTime FechaFin, bool Activa)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param p Promoción
         * @return Promoción para la API
         */
        public static PromocionDto De(Promocion p) => new(p.PrmId, p.PrmDescripcion, p.PrmDescuento, p.PrmFechaIni, p.PrmFechaFin, p.EstaActiva);
    }

    /** Datos para crear o modificar una promoción. */
    public sealed record PromocionRequest(string Descripcion, decimal Descuento, DateTime FechaInicio, DateTime FechaFin)
    {
        /**
         * @brief Convierte la petición al modelo de Core.
         * @param id PRM_ID; 0 al crear
         * @return Promoción lista para el servicio
         */
        public Promocion ALaEntidad(int id = 0) => new()
        {
            PrmId = id,
            PrmDescripcion = Descripcion,
            PrmDescuento = Descuento,
            PrmFechaIni = FechaInicio,
            PrmFechaFin = FechaFin
        };
    }

    /**
     * @brief CRUD de promociones. Consultar es para cualquier usuario; modificar, solo administrador.
     */
    public static class PromocionesEndpoints
    {
        /**
         * @brief Registra las rutas /api/promociones.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapPromociones(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/promociones").WithTags("Promociones").RequireAuthorization();

            grupo.MapGet("/", (PromocionService promociones, string buscar) =>
                    Results.Ok((string.IsNullOrWhiteSpace(buscar) ? promociones.ObtenerPromociones() : promociones.Buscar(buscar)).Select(PromocionDto.De)))
                .WithSummary("Lista las promociones; con ?buscar= filtra por descripción")
                .Produces<IEnumerable<PromocionDto>>();

            grupo.MapGet("/activas", (PromocionService promociones) =>
                    Results.Ok(promociones.ObtenerPromocionesActivas().Select(PromocionDto.De)))
                .WithSummary("Promociones vigentes hoy")
                .Produces<IEnumerable<PromocionDto>>();

            grupo.MapGet("/{id:int}", (int id, PromocionService promociones) =>
                    promociones.ObtenerPorId(id) is { } p ? Results.Ok(PromocionDto.De(p)) : Respuestas.NoEncontrado("Promoción"))
                .WithSummary("Obtiene una promoción")
                .Produces<PromocionDto>()
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPost("/", (PromocionRequest peticion, PromocionService promociones) =>
                {
                    var promocion = peticion.ALaEntidad();
                    promocion.PrmId = promociones.CrearPromocion(promocion);
                    return Results.Created($"/api/promociones/{promocion.PrmId}", PromocionDto.De(promocion));
                })
                .WithSummary("Crea una promoción (administrador)")
                .Produces<PromocionDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapPut("/{id:int}", (int id, PromocionRequest peticion, PromocionService promociones) =>
                    Respuestas.SinContenidoOSiNoExiste(promociones.ActualizarPromocion(peticion.ALaEntidad(id)), "Promoción"))
                .WithSummary("Modifica una promoción (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapDelete("/{id:int}", (int id, PromocionService promociones) =>
                    Respuestas.SinContenidoOSiNoExiste(promociones.EliminarPromocion(id), "Promoción"))
                .WithSummary("Elimina una promoción (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);

            return rutas;
        }
    }
}
