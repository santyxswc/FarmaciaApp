/**
 * @file EmisorTokens.cs
 * @brief Generación de tokens JWT.
 * @author Santiago Caicedo
 */
using System.Security.Claims;
using FarmaciaApp.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FarmaciaApp.Api.Seguridad
{
    /**
     * @brief Crea el token que el cliente envía en cada petición.
     *
     * El token solo identifica al usuario (claim sub). El rol y el estado se consultan
     * en la base de datos en cada petición, así desactivar una cuenta surte efecto de inmediato.
     */
    public sealed class EmisorTokens
    {
        /** Configuración de firma y duración. */
        private readonly OpcionesJwt _opciones;

        /**
         * @brief Crea el emisor.
         * @param opciones Configuración Jwt
         */
        public EmisorTokens(IOptions<OpcionesJwt> opciones) => _opciones = opciones.Value;

        /**
         * @brief Genera un token para un usuario autenticado.
         * @param usuario Usuario que inició sesión
         * @return El token y su fecha de vencimiento
         */
        public (string Token, DateTime Expira) Emitir(Usuario usuario)
        {
            var expira = DateTime.UtcNow.AddMinutes(_opciones.MinutosDeVida);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("sub", usuario.UsuId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    new Claim("name", usuario.Login),
                    new Claim("role", usuario.Rol)
                }),
                Issuer = _opciones.Issuer,
                Audience = _opciones.Audience,
                Expires = expira,
                SigningCredentials = new SigningCredentials(_opciones.ClaveFirma, SecurityAlgorithms.HmacSha256)
            };
            return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
        }
    }
}
