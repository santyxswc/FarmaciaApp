/**
 * @file DbConfig.cs
 * @brief Configuración de la conexión a Oracle.
 * @author Santiago Caicedo
 */
using Microsoft.Extensions.Configuration;

namespace FarmaciaApp.Core.Database
{
    /**
     * @brief Guarda la cadena de conexión leida de appsettings.json.
     *
     * Se inicializa una sola vez al arrancar la aplicación.
     */
    public static class DbConfig
    {
        /** Cadena de conexión de Oracle (ConnectionStrings:OracleConnection). */
        public static string ConnectionString { get; private set; }

        /** Indica si se encontró una cadena de conexión. */
        public static bool Configurada => !string.IsNullOrWhiteSpace(ConnectionString);

        /**
         * @brief Lee la cadena de conexión de la configuración.
         * @param configuration Configuración cargada desde appsettings.json
         */
        public static void Initialize(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("OracleConnection");
        }
    }
}
