using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace FarmaciaApp.Core.Database
{
    public static class DbConfig
    {
        public static string ConnectionString { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("OracleConnection");
        }
    }
}
