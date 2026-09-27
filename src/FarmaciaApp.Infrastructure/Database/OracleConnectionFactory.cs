/**
 * @file OracleConnectionFactory.cs
 * @brief Fábrica de conexiones a Oracle.
 * @author Santiago Caicedo
 */
using System.Data;
using FarmaciaApp.Core.Abstractions;
using Oracle.ManagedDataAccess.Client;

namespace FarmaciaApp.Infrastructure.Database
{
    /**
     * @brief Crea conexiones Oracle con la cadena ConnectionStrings:OracleConnection de appsettings.json.
     */
    public sealed class OracleConnectionFactory : IDbConnectionFactory
    {
        /** Cadena de conexión. */
        private readonly string _cadena;

        /**
         * @brief Crea la fábrica.
         * @param cadenaConexion Cadena de conexión de Oracle (puede ser null si no se configuró)
         */
        public OracleConnectionFactory(string cadenaConexion) => _cadena = cadenaConexion;

        /** Indica si hay una cadena de conexión configurada. */
        public bool Configurada => !string.IsNullOrWhiteSpace(_cadena);

        /**
         * @brief Crea una conexión nueva (cerrada).
         * @return Conexión lista para usar con Dapper
         */
        public IDbConnection Crear() => new OracleConnection(_cadena);
    }
}
