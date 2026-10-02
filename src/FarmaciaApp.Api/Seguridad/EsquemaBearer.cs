/**
 * @file EsquemaBearer.cs
 * @brief Declara el esquema Bearer en el documento OpenAPI.
 * @author Santiago Caicedo
 */
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FarmaciaApp.Api.Seguridad
{
    /**
     * @brief Hace que la documentación interactiva pida el token JWT.
     */
    public sealed class EsquemaBearer : IOpenApiDocumentTransformer
    {
        /**
         * @brief Agrega el esquema de seguridad y lo exige en todas las operaciones.
         * @param documento Documento OpenAPI
         * @param contexto Contexto de la transformación
         * @param cancelacion Token de cancelación
         * @return Tarea completada
         */
        public Task TransformAsync(OpenApiDocument documento, OpenApiDocumentTransformerContext contexto, CancellationToken cancelacion)
        {
            documento.Components ??= new OpenApiComponents();
            documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            documento.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token obtenido en POST /api/auth/login"
            };
            documento.Security ??= new List<OpenApiSecurityRequirement>();
            documento.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
            });
            return Task.CompletedTask;
        }
    }
}
