/**
 * @file ReclamosEndpoints.cs
 * @brief Endpoints de reclamos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Reclamo de un cliente sobre una factura. */
    public sealed record ReclamoDto(decimal Id, DateTime Fecha, string Descripcion, string Estado, decimal FacturaNumero, string Cliente)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param r Reclamo
         * @return Reclamo para la API
         */
        public static ReclamoDto De(Reclamo r) => new(r.RecId, r.RecFecha, r.RecDescripcion, r.RecEstado, r.FacNumFactura, r.ClienteNombre);
    }

    /** Datos para crear o modificar un reclamo. */
    public sealed record ReclamoRequest(decimal FacturaNumero, string Descripcion, string Estado)
    {
        /**
         * @brief Convierte la petición al modelo de Core.
         * @param id Número del reclamo; 0 al crear
         * @return Reclamo listo para el servicio; sin estado queda en el primero de la lista
         */
        public Reclamo ALaEntidad(decimal id = 0) => new()
        {
            RecId = id,
            FacNumFactura = FacturaNumero,
            RecDescripcion = Descripcion,
            RecEstado = Estado ?? ReclamoService.Estados[0]
        };
    }

    /**
     * @brief CRUD de reclamos. Eliminar es solo del administrador.
     */
    public static class ReclamosEndpoints
    {
        /**
         * @brief Registra las rutas /api/reclamos.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapReclamos(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/reclamos").WithTags("Reclamos").RequireAuthorization();

            grupo.MapGet("/", (ReclamoService reclamos, string buscar) =>
                    Results.Ok(reclamos.Buscar(buscar).Select(ReclamoDto.De)))
                .WithSummary("Lista los reclamos; con ?buscar= filtra")
                .Produces<IEnumerable<ReclamoDto>>();

            grupo.MapGet("/estados", () => Results.Ok(ReclamoService.Estados))
                .WithSummary("Estados posibles de un reclamo")
                .Produces<string[]>();

            grupo.MapGet("/{id:decimal}", (decimal id, ReclamoService reclamos) =>
                    reclamos.ObtenerPorId(id) is { } r ? Results.Ok(ReclamoDto.De(r)) : Respuestas.NoEncontrado("Reclamo"))
                .WithSummary("Obtiene un reclamo")
                .Produces<ReclamoDto>()
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPost("/", (ReclamoRequest peticion, ReclamoService reclamos) =>
                {
                    var reclamo = peticion.ALaEntidad();
                    reclamo.RecId = reclamos.CrearReclamo(reclamo);
                    return Results.Created($"/api/reclamos/{reclamo.RecId}", ReclamoDto.De(reclamo));
                })
                .WithSummary("Registra un reclamo sobre una factura")
                .Produces<ReclamoDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest);

            grupo.MapPut("/{id:decimal}", (decimal id, ReclamoRequest peticion, ReclamoService reclamos) =>
                    Respuestas.SinContenidoOSiNoExiste(reclamos.ActualizarReclamo(peticion.ALaEntidad(id)), "Reclamo"))
                .WithSummary("Modifica la descripción y el estado")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapDelete("/{id:decimal}", (decimal id, ReclamoService reclamos) =>
                    Respuestas.SinContenidoOSiNoExiste(reclamos.EliminarReclamo(id), "Reclamo"))
                .WithSummary("Elimina un reclamo (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);

            return rutas;
        }
    }
}
