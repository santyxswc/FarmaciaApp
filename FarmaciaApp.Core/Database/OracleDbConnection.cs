using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace FarmaciaApp.Core.Database
{
    public static class OracleDbConnection
    {
        public static IDbConnection GetConnection()
        {
            return new OracleConnection(DbConfig.ConnectionString);
        }
    }
}
