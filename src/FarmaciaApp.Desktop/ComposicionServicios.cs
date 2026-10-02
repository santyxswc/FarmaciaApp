/**
 * @file ComposicionServicios.cs
 * @brief Registro de dependencias de la aplicación.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Core.Sesion;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FarmaciaApp.Desktop
{
    /**
     * @brief Registro de las dependencias de la aplicación.
     */
    public static class ComposicionServicios
    {
        /**
         * @brief Registra la infraestructura compartida, servicios de negocio y utilidades de la interfaz.
         * @param servicios Colección donde se registran
         * @param cadenaConexion Cadena de conexión de Oracle (puede ser null)
         * @return La misma colección, para encadenar
         */
        public static IServiceCollection AgregarFarmacia(this IServiceCollection servicios, string cadenaConexion)
        {
            servicios.AgregarInfraestructura(cadenaConexion);

            servicios.AddSingleton<ISesionUsuario, SesionUsuario>();
            servicios.AddSingleton<IAuditoria, ServicioAuditoria>();

            servicios.AddSingleton<ClienteService>();
            servicios.AddSingleton<FacturaService>();
            servicios.AddSingleton<MovimientoService>();
            servicios.AddSingleton<PersonaService>();
            servicios.AddSingleton<ProductoService>();
            servicios.AddSingleton<PromocionService>();
            servicios.AddSingleton<ProveedorService>();
            servicios.AddSingleton<ReclamoService>();
            servicios.AddSingleton<ReporteService>();
            servicios.AddSingleton<UsuarioService>();

            servicios.AddSingleton<FabricaVistas>();

            return servicios;
        }
    }
}
