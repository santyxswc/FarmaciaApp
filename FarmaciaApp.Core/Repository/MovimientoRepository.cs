using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class MovimientoRepository
    {
        public void Insert(decimal? usuId, string accion, string detalle)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Execute(@"INSERT INTO TBL_MOVIMIENTO (USU_ID, MOV_FECHA, MOV_ACCION, MOV_DETALLE)
                             VALUES (:UsuId, SYSDATE, :Accion, :Detalle)",
                           new { UsuId = usuId, Accion = accion, Detalle = detalle });
            }
        }

        // Filtros opcionales: usuario y texto (en la accion o el detalle). Maximo 500 filas.
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
