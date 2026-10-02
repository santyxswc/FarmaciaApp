/**
 * @file Composicion.cs
 * @brief Registro de las dependencias de la API.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Core.Sesion;
using FarmaciaApp.Infrastructure;

namespace FarmaciaApp.Api
{
    /**
     * @brief Registra Core e Infrastructure con la vida útil que necesita una API.
     */
    public static class Composicion
    {
        /**
         * @brief Registra repositorios, sesión y servicios de negocio.
         * @param servicios Colección donde se registran
         * @param cadenaConexion Cadena de conexión de Oracle
         * @return La misma colección, para encadenar
         *
         * La sesión, la auditoría y los servicios viven una petición: así cada petición
         * tiene su propio usuario, a diferencia de la aplicación de escritorio, donde es uno solo.
         */
        public static IServiceCollection AgregarFarmacia(this IServiceCollection servicios, string cadenaConexion)
        {
            servicios.AgregarInfraestructura(cadenaConexion);

            servicios.AddScoped<ISesionUsuario, SesionUsuario>();
            servicios.AddScoped<IAuditoria, ServicioAuditoria>();

            servicios.AddScoped<ClienteService>();
            servicios.AddScoped<FacturaService>();
            servicios.AddScoped<MovimientoService>();
            servicios.AddScoped<PersonaService>();
            servicios.AddScoped<ProductoService>();
            servicios.AddScoped<PromocionService>();
            servicios.AddScoped<ProveedorService>();
            servicios.AddScoped<ReclamoService>();
            servicios.AddScoped<ReporteService>();
            servicios.AddScoped<UsuarioService>();
            return servicios;
        }
    }
}
