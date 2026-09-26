/**
 * @file PromocionRepository.cs
 * @brief Acceso a datos de promociones.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    /**
     * @brief Consultas y cambios de TBL_PROMOCION.
     */
    public class PromocionRepository
    {
        /**
         * @brief Obtiene todas las promociones, de la más reciente a la más antigua.
         * @return Promociones
         */
        public IEnumerable<Promocion> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT 
                                   PRM_ID AS PrmId,
                                   PRM_DESCRIPCION AS PrmDescripcion,
                                   PRM_DESCUENTO AS PrmDescuento,
                                   PRM_FECHA_INI AS PrmFechaIni,
                                   PRM_FECHA_FIN AS PrmFechaFin
                               FROM TBL_PROMOCION
                               ORDER BY PRM_FECHA_INI DESC";
                return db.Query<Promocion>(sql);
            }
        }

        /**
         * @brief Busca una promoción.
         * @param id PRM_ID
         * @return Promoción, o null si no existe
         */
        public Promocion GetById(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PRM_ID AS PrmId, PRM_DESCRIPCION AS PrmDescripcion,
                                      PRM_DESCUENTO AS PrmDescuento, PRM_FECHA_INI AS PrmFechaIni,
                                      PRM_FECHA_FIN AS PrmFechaFin
                               FROM TBL_PROMOCION
                               WHERE PRM_ID = :Id";
                return db.QueryFirstOrDefault<Promocion>(sql, new { Id = id });
            }
        }

        /**
         * @brief Registra una promoción.
         * @param p Datos de la promoción
         * @return PRM_ID asignado
         */
        public int Insert(Promocion p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                int newId = db.ExecuteScalar<int>("SELECT NVL(MAX(PRM_ID), 0) + 1 FROM TBL_PROMOCION");

                string sql = @"
                    INSERT INTO TBL_PROMOCION 
                        (PRM_ID, PRM_DESCRIPCION, PRM_DESCUENTO, PRM_FECHA_INI, PRM_FECHA_FIN)
                    VALUES 
                        (:Id, :PrmDescripcion, :PrmDescuento, :PrmFechaIni, :PrmFechaFin)";

                db.Execute(sql, new
                {
                    Id = newId,
                    p.PrmDescripcion,
                    p.PrmDescuento,
                    p.PrmFechaIni,
                    p.PrmFechaFin
                });

                return newId;
            }
        }

        /**
         * @brief Actualiza una promoción.
         * @param p Promoción con los datos nuevos
         * @return true si se actualizo
         */
        public bool Update(Promocion p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    UPDATE TBL_PROMOCION
                    SET PRM_DESCRIPCION = :PrmDescripcion,
                        PRM_DESCUENTO = :PrmDescuento,
                        PRM_FECHA_INI = :PrmFechaIni,
                        PRM_FECHA_FIN = :PrmFechaFin
                    WHERE PRM_ID = :PrmId";

                int rows = db.Execute(sql, new
                {
                    p.PrmDescripcion,
                    p.PrmDescuento,
                    p.PrmFechaIni,
                    p.PrmFechaFin,
                    p.PrmId
                });

                return rows > 0;
            }
        }

        /**
         * @brief Elimina la promoción y su relación con productos.
         * @param id PRM_ID
         * @return true si se elimino
         */
        public bool DeleteCascade(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        db.Execute("DELETE FROM PROMO_PRODU WHERE PRM_ID = :Id", new { Id = id }, tran);

                        int rows = db.Execute("DELETE FROM TBL_PROMOCION WHERE PRM_ID = :Id", new { Id = id }, tran);

                        tran.Commit();
                        return rows > 0;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /**
         * @brief Busca promociones por descripción.
         * @param term Texto a buscar
         * @return Promociones que coinciden
         */
        public IEnumerable<Promocion> SearchByDescription(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PRM_ID AS PrmId, PRM_DESCRIPCION AS PrmDescripcion,
                                      PRM_DESCUENTO AS PrmDescuento, PRM_FECHA_INI AS PrmFechaIni,
                                      PRM_FECHA_FIN AS PrmFechaFin
                               FROM TBL_PROMOCION
                               WHERE LOWER(PRM_DESCRIPCION) LIKE LOWER(:Term)";
                return db.Query<Promocion>(sql, new { Term = $"%{term}%" });
            }
        }

        /**
         * @brief Obtiene las promociones vigentes hoy.
         * @return Promociones activas
         */
        public IEnumerable<Promocion> GetActivas()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PRM_ID AS PrmId, PRM_DESCRIPCION AS PrmDescripcion,
                                      PRM_DESCUENTO AS PrmDescuento, PRM_FECHA_INI AS PrmFechaIni,
                                      PRM_FECHA_FIN AS PrmFechaFin
                               FROM TBL_PROMOCION
                               WHERE SYSDATE BETWEEN PRM_FECHA_INI AND PRM_FECHA_FIN
                               ORDER BY PRM_FECHA_INI DESC";
                return db.Query<Promocion>(sql);
            }
        }
    }
}
