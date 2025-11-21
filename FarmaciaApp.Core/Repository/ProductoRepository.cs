using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class ProductoRepository
    {
        public IEnumerable<Producto> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT 
                                   PRO_ID    AS ProId,
                                   PRO_NOMBRE AS ProNombre,
                                   PRO_PRECIO AS ProPrecio,
                                   PRO_STOCK AS ProStock,
                                   PRO_DESCRIPCION AS ProDescripcion
                               FROM TBL_PRODUCTO
                               ORDER BY PRO_NOMBRE";
                return db.Query<Producto>(sql);
            }
        }

        public Producto GetById(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PRO_ID AS ProId, PRO_NOMBRE AS ProNombre, PRO_PRECIO AS ProPrecio,
                                      PRO_STOCK AS ProStock, PRO_DESCRIPCION AS ProDescripcion
                               FROM TBL_PRODUCTO
                               WHERE PRO_ID = :Id";
                return db.QueryFirstOrDefault<Producto>(sql, new { Id = id });
            }
        }

        public int Insert(Producto p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                // 1) Obtener NEXTVAL
                int newId = db.ExecuteScalar<int>("SELECT SEQ_PRODUCTO.NEXTVAL FROM DUAL");

                // 2) Insert con el id obtenido
                string sql = @"
                    INSERT INTO TBL_PRODUCTO (PRO_ID, PRO_NOMBRE, PRO_PRECIO, PRO_STOCK, PRO_DESCRIPCION)
                    VALUES (:Id, :ProNombre, :ProPrecio, :ProStock, :ProDescripcion)";

                db.Execute(sql, new
                {
                    Id = newId,
                    ProNombre = p.ProNombre,
                    ProPrecio = p.ProPrecio,
                    ProStock = p.ProStock,
                    ProDescripcion = p.ProDescripcion
                });

                return newId;
            }
        }

        public bool Update(Producto p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    UPDATE TBL_PRODUCTO
                    SET PRO_NOMBRE = :ProNombre,
                        PRO_PRECIO = :ProPrecio,
                        PRO_STOCK = :ProStock,
                        PRO_DESCRIPCION = :ProDescripcion
                    WHERE PRO_ID = :ProId";

                int rows = db.Execute(sql, new
                {
                    ProNombre = p.ProNombre,
                    ProPrecio = p.ProPrecio,
                    ProStock = p.ProStock,
                    ProDescripcion = p.ProDescripcion,
                    ProId = p.ProId
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
                        // Borrar relaciones en tablas intermedias (orden seguro)
                        db.Execute("DELETE FROM PROMO_PRODU WHERE PRO_ID = :Id", new { Id = id }, tran);
                        db.Execute("DELETE FROM PROVEE_PRODUC WHERE PRO_ID = :Id", new { Id = id }, tran);
                        db.Execute("DELETE FROM FACTU_PRODUC WHERE PRO_ID = :Id", new { Id = id }, tran);

                        // Finalmente borrar el producto
                        int rows = db.Execute("DELETE FROM TBL_PRODUCTO WHERE PRO_ID = :Id", new { Id = id }, tran);

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

        public IEnumerable<Producto> SearchByName(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PRO_ID AS ProId, PRO_NOMBRE AS ProNombre, PRO_PRECIO AS ProPrecio,
                                      PRO_STOCK AS ProStock, PRO_DESCRIPCION AS ProDescripcion
                               FROM TBL_PRODUCTO
                               WHERE LOWER(PRO_NOMBRE) LIKE LOWER(:Term)";
                return db.Query<Producto>(sql, new { Term = $"%{term}%" });
            }
        }
    }
}
