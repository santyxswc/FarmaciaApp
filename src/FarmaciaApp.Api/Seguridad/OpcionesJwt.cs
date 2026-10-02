/**
 * @file OpcionesJwt.cs
 * @brief Configuración de los tokens JWT.
 * @author Santiago Caicedo
 */
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FarmaciaApp.Api.Seguridad
{
    /**
     * @brief Sección "Jwt" de la configuración.
     */
    public sealed class OpcionesJwt
    {
        /** Largo mínimo de la clave de firma, en caracteres. */
        public const int LargoMinimoClave = 32;

        /** Clave secreta con la que se firman los tokens. */
        public string Key { get; set; } = "";
        /** Quién emite los tokens. */
        public string Issuer { get; set; } = "FarmaciaApp";
        /** Para quién se emiten. */
        public string Audience { get; set; } = "FarmaciaApp";
        /** Minutos que dura un token. */
        public int MinutosDeVida { get; set; } = 60;

        /** Clave de firma derivada de Key. */
        public SymmetricSecurityKey ClaveFirma => new(Encoding.UTF8.GetBytes(Key));

        /**
         * @brief Indica si la configuración sirve para firmar tokens.
         * @return true si la clave tiene el largo mínimo
         */
        public bool EsValida() => Key.Length >= LargoMinimoClave;

        /**
         * @brief Reglas con las que se valida cada token recibido.
         * @return Parámetros de validación
         */
        public TokenValidationParameters ParametrosDeValidacion() => new()
        {
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = ClaveFirma,
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }
}
