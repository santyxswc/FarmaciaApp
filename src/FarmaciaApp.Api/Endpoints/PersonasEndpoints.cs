/**
 * @file PersonasEndpoints.cs
 * @brief Endpoints de personas.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Persona con sus roles. */
    public sealed record PersonaDto(int Id, string Nombre, string Apellido, string Direccion, string Telefono, string Email, bool EsCliente, bool EsVendedor)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param p Persona
         * @return Persona para la API
         */
        public static PersonaDto De(Persona p) => new(p.PerId, p.PerNombre, p.PerApellido, p.PerDireccion, p.PerTelefono, p.PerEmail, p.EsCliente, p.EsVendedor);
    }

    /** Datos personales para crear o modificar una persona. */
    public sealed record PersonaRequest(string Nombre, string Apellido, string Direccion, string Telefono, string Email)
    {
        /**
         * @brief Convierte la petición al modelo de Core.
         * @param id PER_ID; 0 al crear
         * @return Persona lista para el servicio
         */
        public Persona ALaEntidad(int id = 0) => new()
        {
            PerId = id,
            PerNombre = Nombre,
            PerApellido = Apellido,
            PerDireccion = Direccion,
            PerTelefono = Telefono,
            PerEmail = Email
        };
    }

    /** Marca o desmarca a una persona como vendedor. */
    public sealed record VendedorRequest(bool EsVendedor);

    /**
     * @brief CRUD de personas. Cambiar el rol de vendedor y eliminar son del administrador.
     */
    public static class PersonasEndpoints
    {
        /**
         * @brief Registra las rutas /api/personas.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapPersonas(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/personas").WithTags("Personas").RequireAuthorization();

            grupo.MapGet("/", (PersonaService personas, string buscar) =>
                    Results.Ok((string.IsNullOrWhiteSpace(buscar) ? personas.ObtenerPersonas() : personas.Buscar(buscar)).Select(PersonaDto.De)))
                .WithSummary("Lista las personas; con ?buscar= filtra por nombre")
                .Produces<IEnumerable<PersonaDto>>();

            grupo.MapGet("/{id:int}", (int id, PersonaService personas) =>
                    personas.ObtenerPorId(id) is { } p ? Results.Ok(PersonaDto.De(p)) : Respuestas.NoEncontrado("Persona"))
                .WithSummary("Obtiene una persona")
                .Produces<PersonaDto>()
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPost("/", (PersonaRequest peticion, PersonaService personas) =>
                {
                    var persona = peticion.ALaEntidad();
                    persona.PerId = personas.CrearPersona(persona);
                    return Results.Created($"/api/personas/{persona.PerId}", PersonaDto.De(persona));
                })
                .WithSummary("Registra una persona")
                .Produces<PersonaDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest);

            grupo.MapPut("/{id:int}", (int id, PersonaRequest peticion, PersonaService personas) =>
                    Respuestas.SinContenidoOSiNoExiste(personas.ActualizarPersona(peticion.ALaEntidad(id)), "Persona"))
                .WithSummary("Modifica los datos personales")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPut("/{id:int}/vendedor", (int id, VendedorRequest peticion, PersonaService personas) =>
                {
                    personas.AsignarVendedor(id, peticion.EsVendedor);
                    return Results.NoContent();
                })
                .WithSummary("Marca o desmarca a la persona como vendedor (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status409Conflict);

            grupo.MapDelete("/{id:int}", (int id, PersonaService personas) =>
                    Respuestas.SinContenidoOSiNoExiste(personas.EliminarPersona(id), "Persona"))
                .WithSummary("Elimina una persona sin facturas ni cuenta (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            return rutas;
        }
    }
}
