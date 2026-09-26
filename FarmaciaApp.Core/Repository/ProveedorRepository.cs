/**
 * @file ProveedorRepository.cs
 * @brief Acceso a datos de proveedores.
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
     * @brief Consultas y cambios de TBL_PROVEEDOR.
     */
    public class ProveedorRepository
    {
        /**
         * @brief Obtiene todos los proveedores ordenados por nombre.
         * @return Proveedores
         */
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

        /**
         * @brief Busca un proveedor.
         * @param id Identificador
         * @return Proveedor, o null si no existe
         */
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

        /**
         * @brief Registra un proveedor.
         * @param p Datos del proveedor
         * @return Identificador asignado
         */
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

        /**
         * @brief Actualiza un proveedor.
         * @param p Proveedor con los datos nuevos
         * @return true si se actualizo
         */
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

        /**
         * @brief Elimina el proveedor y su relación con productos.
         * @param id Identificador
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
                        db.Execute("DELETE FROM PROVEE_PRODUC WHERE PROV_ID = :Id", new { Id = id }, tran);

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

        /**
         * @brief Busca proveedores por nombre o contacto.
         * @param term Texto a buscar
         * @return Proveedores que coinciden
         */
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
