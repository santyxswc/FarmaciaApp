/**
 * @file MovimientoRepository.cs
 * @brief Acceso a datos del registro de movimientos.
 * @author Santiago Caicedo
 */
using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    /**
     * @brief Escritura y consulta de TBL_MOVIMIENTO.
     */
    public class MovimientoRepository
    {
        /**
         * @brief Registra un movimiento con la fecha actual del servidor.
         * @param usuId Usuario que hizo la acción, o null si no hay sesión
         * @param accion Acción realizada
         * @param detalle Detalle de la acción
         */
        public void Insert(decimal? usuId, string accion, string detalle)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Execute(@"INSERT INTO TBL_MOVIMIENTO (USU_ID, MOV_FECHA, MOV_ACCION, MOV_DETALLE)
                             VALUES (:UsuId, SYSDATE, :Accion, :Detalle)",
                           new { UsuId = usuId, Accion = accion, Detalle = detalle });
            }
        }

        /**
         * @brief Busca movimientos en un rango de fechas.
         * @param desde Fecha inicial
         * @param hastaExclusivo Fecha final (no incluida)
         * @param usuId Filtrar por usuario, o null para todos
         * @param texto Texto a buscar en la acción o el detalle, o null
         * @return Hasta 500 movimientos, del más reciente al más antiguo
         */
        public IEnumerable<Movimiento> Buscar(DateTime desde, DateTime hastaExclusivo, decimal? usuId, string texto)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    SELECT M.MOV_ID AS MovId,
                           M.MOV_FECHA AS Fecha,
                           NVL(U.USU_LOGIN, '(sin sesión)') AS Usuario,
                           U.USU_ROL AS Rol,
                           M.MOV_ACCION AS Accion,
                           M.MOV_DETALLE AS Detalle
                    FROM TBL_MOVIMIENTO M
                    LEFT JOIN TBL_USUARIO U ON U.USU_ID = M.USU_ID
                    WHERE M.MOV_FECHA >= :Desde AND M.MOV_FECHA < :Hasta
                      AND (:UsuId IS NULL OR M.USU_ID = :UsuId)
                      AND (:Texto IS NULL OR LOWER(M.MOV_ACCION) LIKE :Texto OR LOWER(M.MOV_DETALLE) LIKE :Texto)
                    ORDER BY M.MOV_FECHA DESC, M.MOV_ID DESC
                    FETCH FIRST 500 ROWS ONLY";

                return db.Query<Movimiento>(sql, new
                {
                    Desde = desde,
                    Hasta = hastaExclusivo,
                    UsuId = usuId,
                    Texto = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto.Trim().ToLowerInvariant()}%"
                });
            }
        }
    }
}
