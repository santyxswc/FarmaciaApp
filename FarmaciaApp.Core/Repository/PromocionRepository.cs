using System;
using System.Collections.Generic;
using System.Text;

using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class PromocionRepository
    {
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

        public bool DeleteCascade(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        // Eliminar relaciones producto-promoción
                        db.Execute("DELETE FROM PROMO_PRODU WHERE PRM_ID = :Id", new { Id = id }, tran);

                        // Eliminar promoción
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