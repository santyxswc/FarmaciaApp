/**
 * @file AuthEndpoints.cs
 * @brief Endpoints de autenticación.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Api.Seguridad;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Api.Endpoints
{
    /**
     * @brief Inicio de sesión y consulta del usuario autenticado.
     */
    public static class AuthEndpoints
    {
        /**
         * @brief Registra las rutas /api/auth.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/auth").WithTags("Autenticación");

            grupo.MapPost("/login", Login)
                .RequireRateLimiting("login")
                .WithSummary("Inicia sesión y devuelve un token JWT")
                .Produces<LoginResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status429TooManyRequests);

            grupo.MapGet("/yo", Yo)
                .RequireAuthorization()
                .WithSummary("Datos del usuario del token")
                .Produces<UsuarioDto>();

            return rutas;
        }

        /**
         * @brief Verifica las credenciales y emite el token.
         * @param peticion Usuario y contraseña
         * @param usuarios Servicio de usuarios
         * @param emisor Emisor de tokens
         * @return 200 con el token; 401 si las credenciales no son correctas; 403 si la cuenta está desactivada
         */
        private static IResult Login(LoginRequest peticion, UsuarioService usuarios, EmisorTokens emisor)
        {
            if (string.IsNullOrWhiteSpace(peticion.Login) || string.IsNullOrEmpty(peticion.Clave))
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Datos inválidos",
                    detail: "El usuario y la contraseña son obligatorios.");

            Core.Models.Usuario usuario;
            try
            {
                usuario = usuarios.IniciarSesion(peticion.Login, peticion.Clave);
            }
            catch (CuentaDesactivadaException ex)
            {
                return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Cuenta desactivada", detail: ex.Message);
            }

            if (usuario == null)
                return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Credenciales incorrectas",
                    detail: "Usuario o contraseña incorrectos.");

            var (token, expira) = emisor.Emitir(usuario);
            return Results.Ok(new LoginResponse(token, expira, UsuarioDto.De(usuario)));
        }

        /**
         * @brief Devuelve el usuario de la sesión de la petición.
         * @param sesion Sesión abierta por el middleware
         * @return 200 con los datos del usuario
         */
        private static IResult Yo(ISesionUsuario sesion) => Results.Ok(UsuarioDto.De(sesion.Usuario));
    }
}
