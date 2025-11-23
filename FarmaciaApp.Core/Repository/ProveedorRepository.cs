using System;
using System.Collections.Generic;
using System.Text;

using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class ProveedorRepository
    {
        public IEnumerable<Proveedor> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT 
                                   PRO_ID AS ProId,
                                   PRO_NOMBRE AS ProNombre,
                                   PRO_CONTACTO AS ProContacto,
                                   PRO_TELEFONO AS ProTelefono
                               FROM TBL_PROVEEDOR
                               ORDER BY PRO_NOMBRE";
                return db.Query<Proveedor>(sql);
            }
        }

        public Proveedor GetById(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PRO_ID AS ProId, PRO_NOMBRE AS ProNombre,
                                      PRO_CONTACTO AS ProContacto, PRO_TELEFONO AS ProTelefono
                               FROM TBL_PROVEEDOR
                               WHERE PRO_ID = :Id";
                return db.QueryFirstOrDefault<Proveedor>(sql, new { Id = id });
            }
        }

        public int Insert(Proveedor p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                int newId = db.ExecuteScalar<int>("SELECT NVL(MAX(PRO_ID), 0) + 1 FROM TBL_PROVEEDOR");

                string sql = @"
                    INSERT INTO TBL_PROVEEDOR 
                        (PRO_ID, PRO_NOMBRE, PRO_CONTACTO, PRO_TELEFONO)
                    VALUES 
                        (:Id, :ProNombre, :ProContacto, :ProTelefono)";

                db.Execute(sql, new
                {
                    Id = newId,
                    p.ProNombre,
                    p.ProContacto,
                    p.ProTelefono
                });

                return newId;
            }
        }

        public bool Update(Proveedor p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    UPDATE TBL_PROVEEDOR
                    SET PRO_NOMBRE = :ProNombre,
                        PRO_CONTACTO = :ProContacto,
                        PRO_TELEFONO = :ProTelefono
                    WHERE PRO_ID = :ProId";

                int rows = db.Execute(sql, new
                {
                    p.ProNombre,
                    p.ProContacto,
                    p.ProTelefono,
                    p.ProId
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
                        // Eliminar relaciones proveedor-producto
                        db.Execute("DELETE FROM PROVEE_PRODUC WHERE PROV_ID = :Id", new { Id = id }, tran);

                        // Eliminar proveedor
                        int rows = db.Execute("DELETE FROM TBL_PROVEEDOR WHERE PRO_ID = :Id", new { Id = id }, tran);

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

        public IEnumerable<Proveedor> SearchByName(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PRO_ID AS ProId, PRO_NOMBRE AS ProNombre,
                                      PRO_CONTACTO AS ProContacto, PRO_TELEFONO AS ProTelefono
                               FROM TBL_PROVEEDOR
                               WHERE LOWER(PRO_NOMBRE) LIKE LOWER(:Term)
                                  OR LOWER(PRO_CONTACTO) LIKE LOWER(:Term)";
                return db.Query<Proveedor>(sql, new { Term = $"%{term}%" });
            }
        }
    }
}