/**
 * @file IDbConnectionFactory.cs
 * @brief Contrato para obtener conexiones a la base de datos.
 * @author Santiago Caicedo
 */
using System.Data;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Crea conexiones nuevas; cada repositorio abre y cierra la suya por operación.
     */
    public interface IDbConnectionFactory
    {
        /** Indica si hay una cadena de conexión configurada. */
        bool Configurada { get; }

        /**
         * @brief Crea una conexión nueva (cerrada).
         * @return Conexión lista para usar con Dapper
         */
        IDbConnection Crear();
    }
}
