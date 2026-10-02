/**
 * @file OracleHealthCheck.cs
 * @brief Chequeo de salud de la conexión a Oracle.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FarmaciaApp.Api.Salud
{
    /**
     * @brief Comprueba que la base de datos acepta consultas.
     */
    public sealed class OracleHealthCheck : IHealthCheck
    {
        /** Fábrica de conexiones. */
        private readonly IDbConnectionFactory _conexiones;

        /**
         * @brief Crea el chequeo.
         * @param conexiones Fábrica de conexiones
         */
        public OracleHealthCheck(IDbConnectionFactory conexiones) => _conexiones = conexiones;

        /**
         * @brief Ejecuta SELECT 1 FROM DUAL.
         * @param contexto Contexto del chequeo
         * @param cancelacion Token de cancelación
         * @return Saludable si Oracle responde; no saludable con el motivo si no
         */
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext contexto, CancellationToken cancelacion = default)
        {
            try
            {
                using var conexion = _conexiones.Crear();
                conexion.Open();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT 1 FROM DUAL";
                comando.ExecuteScalar();
                return Task.FromResult(HealthCheckResult.Healthy("Oracle responde"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Oracle no responde", ex));
            }
        }
    }
}
