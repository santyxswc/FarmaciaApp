/**
 * @file RegistroInfraestructura.cs
 * @brief Registro de las implementaciones de acceso a datos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Infrastructure.Database;
using FarmaciaApp.Infrastructure.Repositories;
using FarmaciaApp.Infrastructure.Seguridad;
using Microsoft.Extensions.DependencyInjection;

namespace FarmaciaApp.Infrastructure
{
    /**
     * @brief Registra la conexión a Oracle, los repositorios y el hash de contraseñas.
     *
     * Lo comparten la aplicación de escritorio y la API. No guardan estado entre llamadas:
     * cada consulta abre su propia conexión.
     */
    public static class RegistroInfraestructura
    {
        /**
         * @brief Registra las implementaciones de los contratos de Core.
         * @param servicios Colección donde se registran
         * @param cadenaConexion Cadena de conexión de Oracle (puede ser null)
         * @return La misma colección, para encadenar
         */
        public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, string cadenaConexion)
        {
            servicios.AddSingleton<IDbConnectionFactory>(new OracleConnectionFactory(cadenaConexion));
            servicios.AddSingleton<IHasherClaves, HasherPbkdf2>();
            servicios.AddSingleton<IClienteRepository, ClienteRepository>();
            servicios.AddSingleton<IFacturaRepository, FacturaRepository>();
            servicios.AddSingleton<IMovimientoRepository, MovimientoRepository>();
            servicios.AddSingleton<IPersonaRepository, PersonaRepository>();
            servicios.AddSingleton<IProductoRepository, ProductoRepository>();
            servicios.AddSingleton<IPromocionRepository, PromocionRepository>();
            servicios.AddSingleton<IProveedorRepository, ProveedorRepository>();
            servicios.AddSingleton<IReclamoRepository, ReclamoRepository>();
            servicios.AddSingleton<IReporteRepository, ReporteRepository>();
            servicios.AddSingleton<IUsuarioRepository, UsuarioRepository>();
            return servicios;
        }
    }
}
