/**
 * @file ClientesEndpoints.cs
 * @brief Endpoints de clientes.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Cliente registrado. */
    public sealed record ClienteDto(decimal Id, string Nombre, string Apellido, string Direccion, string Telefono, string Email)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param c Cliente
         * @return Cliente para la API
         */
        public static ClienteDto De(Cliente c) => new(c.PerId, c.PerNombre, c.PerApellido, c.PerDireccion, c.PerTelefono, c.PerEmail);
    }

    /** Datos para crear o modificar un cliente. */
    public sealed record ClienteRequest(string Nombre, string Apellido, string Direccion, string Telefono, string Email)
    {
        /**
         * @brief Convierte la petición al modelo de Core.
         * @param id PER_ID; 0 al crear
         * @return Cliente listo para el servicio
         */
        public Cliente ALaEntidad(decimal id = 0) => new()
        {
            PerId = id,
            PerNombre = Nombre,
            PerApellido = Apellido,
            PerDireccion = Direccion,
            PerTelefono = Telefono,
            PerEmail = Email
        };
    }

    /**
     * @brief CRUD de clientes. Eliminar es solo del administrador.
     */
    public static class ClientesEndpoints
    {
        /**
         * @brief Registra las rutas /api/clientes.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapClientes(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/clientes").WithTags("Clientes").RequireAuthorization();

            grupo.MapGet("/", (ClienteService clientes, string buscar) =>
                    Results.Ok(clientes.Buscar(buscar).Select(ClienteDto.De)))
                .WithSummary("Lista los clientes; con ?buscar= filtra por nombre o apellido")
                .Produces<IEnumerable<ClienteDto>>();

            grupo.MapGet("/{id:decimal}", (decimal id, ClienteService clientes) =>
                    clientes.ObtenerPorId(id) is { } c ? Results.Ok(ClienteDto.De(c)) : Respuestas.NoEncontrado("Cliente"))
                .WithSummary("Obtiene un cliente")
                .Produces<ClienteDto>()
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPost("/", (ClienteRequest peticion, ClienteService clientes) =>
                {
                    var cliente = peticion.ALaEntidad();
                    cliente.PerId = clientes.CrearCliente(cliente);
                    return Results.Created($"/api/clientes/{cliente.PerId}", ClienteDto.De(cliente));
                })
                .WithSummary("Registra un cliente")
                .Produces<ClienteDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest);

            grupo.MapPut("/{id:decimal}", (decimal id, ClienteRequest peticion, ClienteService clientes) =>
                    Respuestas.SinContenidoOSiNoExiste(clientes.ActualizarCliente(peticion.ALaEntidad(id)), "Cliente"))
                .WithSummary("Modifica un cliente")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapDelete("/{id:decimal}", (decimal id, ClienteService clientes) =>
                    Respuestas.SinContenidoOSiNoExiste(clientes.EliminarCliente(id), "Cliente"))
                .WithSummary("Elimina un cliente sin facturas (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            return rutas;
        }
    }
}
