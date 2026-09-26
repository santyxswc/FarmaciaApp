/**
 * @file ProductoRepository.cs
 * @brief Acceso a datos de productos.
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
     * @brief Consultas y cambios de TBL_PRODUCTO.
     */
    public class ProductoRepository
    {
        /**
         * @brief Obtiene todos los productos ordenados por nombre.
         * @return Productos
         */
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

        /**
         * @brief Busca un producto.
         * @param id PRO_ID
         * @return Producto, o null si no existe
         */
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

        /**
         * @brief Registra un producto.
         * @param p Datos del producto
         * @return PRO_ID asignado por SEQ_PRODUCTO
         */
        public int Insert(Producto p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                int newId = db.ExecuteScalar<int>("SELECT SEQ_PRODUCTO.NEXTVAL FROM DUAL");
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

        /**
         * @brief Actualiza un producto.
         * @param p Producto con los datos nuevos
         * @return true si se actualizo
         */
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

        /**
         * @brief Cuenta las líneas de factura en las que aparece el producto.
         * @param id PRO_ID
         * @return Número de líneas
         */
        public int CountVentas(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM FACTU_PRODUC WHERE PRO_ID = :Id", new { Id = id });
            }
        }

        /**
         * @brief Elimina el producto y sus relaciones con promociones y proveedores.
         * @param id PRO_ID
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
                        db.Execute("DELETE FROM PROMO_PRODU WHERE PRO_ID = :Id", new { Id = id }, tran);
                        db.Execute("DELETE FROM PROVEE_PRODUC WHERE PRO_ID = :Id", new { Id = id }, tran);

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

        /**
         * @brief Busca productos por nombre.
         * @param term Texto a buscar
         * @return Productos que coinciden
         */
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
