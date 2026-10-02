/**
 * @file UsuariosEndpoints.cs
 * @brief Endpoints de administración de usuarios.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Cuenta de usuario para el administrador; nunca incluye el hash ni la sal. */
    public sealed record UsuarioAdminDto(decimal Id, string Login, string Nombre, string Rol, bool Activo, DateTime? UltimoIngreso, decimal? PersonaId)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param u Usuario
         * @return Cuenta para la API
         */
        public static UsuarioAdminDto De(Usuario u) => new(u.UsuId, u.Login, u.Nombre, u.Rol, u.Activo, u.UltimoIngreso, u.PerId);
    }

    /** Datos de una cuenta nueva. */
    public sealed record CrearUsuarioRequest(string Login, string Clave, string Confirmacion, string Rol, decimal? PersonaId);

    /** Id de la cuenta creada. */
    public sealed record UsuarioCreadoDto(decimal Id);

    /** Activa o desactiva una cuenta. */
    public sealed record EstadoRequest(bool Activo);

    /** Contraseña nueva de otra cuenta. */
    public sealed record RestablecerClaveRequest(string Nueva, string Confirmacion);

    /** Cambio de la propia contraseña. */
    public sealed record CambiarClaveRequest(string Actual, string Nueva, string Confirmacion);

    /**
     * @brief Administración de cuentas. Todo es solo del administrador, salvo cambiar la propia contraseña.
     */
    public static class UsuariosEndpoints
    {
        /**
         * @brief Registra las rutas /api/usuarios y PUT /api/auth/clave.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapUsuarios(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/usuarios").WithTags("Usuarios").RequireAuthorization();

            grupo.MapGet("/", (UsuarioService usuarios) => Results.Ok(usuarios.ObtenerUsuarios().Select(UsuarioAdminDto.De)))
                .WithSummary("Lista las cuentas (administrador)")
                .Produces<IEnumerable<UsuarioAdminDto>>()
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapPost("/", (CrearUsuarioRequest p, UsuarioService usuarios) =>
                {
                    decimal id = usuarios.CrearUsuario(p.Login, p.Clave, p.Confirmacion, p.Rol, p.PersonaId);
                    return Results.Created("/api/usuarios", new UsuarioCreadoDto(id));
                })
                .WithSummary("Crea una cuenta; un empleado debe estar asociado a una persona (administrador)")
                .Produces<UsuarioCreadoDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status409Conflict);

            grupo.MapPut("/{id:decimal}/estado", (decimal id, EstadoRequest p, UsuarioService usuarios) =>
                {
                    usuarios.CambiarEstado(id, p.Activo);
                    return Results.NoContent();
                })
                .WithSummary("Activa o desactiva una cuenta; no se puede desactivar la propia ni al último administrador (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status409Conflict);

            grupo.MapPut("/{id:decimal}/clave", (decimal id, RestablecerClaveRequest p, UsuarioService usuarios) =>
                {
                    usuarios.RestablecerClave(id, p.Nueva, p.Confirmacion);
                    return Results.NoContent();
                })
                .WithSummary("Asigna una contraseña nueva a una cuenta (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status409Conflict);

            rutas.MapPut("/api/auth/clave", (CambiarClaveRequest p, UsuarioService usuarios) =>
                {
                    usuarios.CambiarMiClave(p.Actual, p.Nueva, p.Confirmacion);
                    return Results.NoContent();
                })
                .WithTags("Autenticación")
                .RequireAuthorization()
                .WithSummary("Cambia la contraseña del usuario del token")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict);

            return rutas;
        }
    }
}
