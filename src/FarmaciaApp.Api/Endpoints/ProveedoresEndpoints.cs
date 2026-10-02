/**
 * @file ProveedoresEndpoints.cs
 * @brief Endpoints de proveedores.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Proveedor. */
    public sealed record ProveedorDto(int Id, string Nombre, string Contacto, string Telefono)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param p Proveedor
         * @return Proveedor para la API
         */
        public static ProveedorDto De(Proveedor p) => new(p.ProId, p.ProNombre, p.ProContacto, p.ProTelefono);
    }

    /** Datos para crear o modificar un proveedor. */
    public sealed record ProveedorRequest(string Nombre, string Contacto, string Telefono)
    {
        /**
         * @brief Convierte la petición al modelo de Core.
         * @param id PRO_ID; 0 al crear
         * @return Proveedor listo para el servicio
         */
        public Proveedor ALaEntidad(int id = 0) => new() { ProId = id, ProNombre = Nombre, ProContacto = Contacto, ProTelefono = Telefono };
    }

    /**
     * @brief CRUD de proveedores. Consultar es para cualquier usuario; modificar, solo administrador.
     */
    public static class ProveedoresEndpoints
    {
        /**
         * @brief Registra las rutas /api/proveedores.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapProveedores(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/proveedores").WithTags("Proveedores").RequireAuthorization();

            grupo.MapGet("/", (ProveedorService proveedores, string buscar) =>
                    Results.Ok((string.IsNullOrWhiteSpace(buscar) ? proveedores.ObtenerProveedores() : proveedores.Buscar(buscar)).Select(ProveedorDto.De)))
                .WithSummary("Lista los proveedores; con ?buscar= filtra por nombre")
                .Produces<IEnumerable<ProveedorDto>>();

            grupo.MapGet("/{id:int}", (int id, ProveedorService proveedores) =>
                    proveedores.ObtenerPorId(id) is { } p ? Results.Ok(ProveedorDto.De(p)) : Respuestas.NoEncontrado("Proveedor"))
                .WithSummary("Obtiene un proveedor")
                .Produces<ProveedorDto>()
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPost("/", (ProveedorRequest peticion, ProveedorService proveedores) =>
                {
                    var proveedor = peticion.ALaEntidad();
                    proveedor.ProId = proveedores.CrearProveedor(proveedor);
                    return Results.Created($"/api/proveedores/{proveedor.ProId}", ProveedorDto.De(proveedor));
                })
                .WithSummary("Registra un proveedor (administrador)")
                .Produces<ProveedorDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapPut("/{id:int}", (int id, ProveedorRequest peticion, ProveedorService proveedores) =>
                    Respuestas.SinContenidoOSiNoExiste(proveedores.ActualizarProveedor(peticion.ALaEntidad(id)), "Proveedor"))
                .WithSummary("Modifica un proveedor (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapDelete("/{id:int}", (int id, ProveedorService proveedores) =>
                    Respuestas.SinContenidoOSiNoExiste(proveedores.EliminarProveedor(id), "Proveedor"))
                .WithSummary("Elimina un proveedor (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);

            return rutas;
        }
    }
}
