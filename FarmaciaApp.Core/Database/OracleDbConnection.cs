/**
 * @file OracleDbConnection.cs
 * @brief Fábrica de conexiones a Oracle.
 * @author Santiago Caicedo
 */
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace FarmaciaApp.Core.Database
{
    /** Crea las conexiones que usan los repositorios. */
    public static class OracleDbConnection
    {
        /**
         * @brief Crea una conexión nueva (cerrada) con la cadena de DbConfig.
         * @return Conexion lista para usar con Dapper
         */
        public static IDbConnection GetConnection()
        {
            return new OracleConnection(DbConfig.ConnectionString);
        }
    }
}
