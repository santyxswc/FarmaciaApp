/**
 * @file ClienteRepository.cs
 * @brief Acceso a datos de clientes.
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
     * @brief Consultas y cambios de clientes en TBL_PERSONA y TBL_CLIENTE.
     */
    public class ClienteRepository
    {
        /** Consulta base de clientes con sus datos personales. */
        private const string ClienteSelectSql = @"
            SELECT 
                P.PER_ID AS PerId, 
                P.PER_NOMBRE AS PerNombre, 
                P.PER_APELLIDO AS PerApellido,
                P.PER_DIRECCION AS PerDireccion, 
                P.PER_TELEFONO AS PerTelefono, 
                P.PER_EMAIL AS PerEmail
            FROM 
                TBL_PERSONA P
            INNER JOIN 
                TBL_CLIENTE C ON P.PER_ID = C.PER_ID";

        /**
         * @brief Obtiene todos los clientes ordenados por apellido.
         * @return Clientes
         */
        public IEnumerable<Cliente> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ClienteSelectSql + " ORDER BY P.PER_APELLIDO, P.PER_NOMBRE";
                return db.Query<Cliente>(sql);
            }
        }

        /**
         * @brief Busca un cliente.
         * @param id PER_ID del cliente
         * @return Cliente, o null si no existe
         */
        public Cliente GetById(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ClienteSelectSql + " WHERE P.PER_ID = :Id";
                return db.QueryFirstOrDefault<Cliente>(sql, new { Id = id });
            }
        }

        /**
         * @brief Registra una persona y la marca como cliente en una transacción.
         * @param c Datos del cliente
         * @return PER_ID asignado por SEQ_PERSONA
         */
        public decimal Insert(Cliente c)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        decimal newId = db.ExecuteScalar<decimal>("SELECT SEQ_PERSONA.NEXTVAL FROM DUAL", null, tran);

                        string personaSql = @"
                            INSERT INTO TBL_PERSONA (PER_ID, PER_NOMBRE, PER_APELLIDO, PER_DIRECCION, PER_TELEFONO, PER_EMAIL)
                            VALUES (:Id, :PerNombre, :PerApellido, :PerDireccion, :PerTelefono, :PerEmail)";

                        db.Execute(personaSql, new
                        {
                            Id = newId,
                            c.PerNombre,
                            c.PerApellido,
                            c.PerDireccion,
                            c.PerTelefono,
                            c.PerEmail
                        }, tran);

                        string clienteSql = @"INSERT INTO TBL_CLIENTE (PER_ID) VALUES (:Id)";
                        db.Execute(clienteSql, new { Id = newId }, tran);

                        tran.Commit();
                        return newId;
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
         * @brief Actualiza los datos personales de un cliente.
         * @param c Cliente con los datos nuevos
         * @return true si se actualizo
         */
        public bool Update(Cliente c)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    UPDATE TBL_PERSONA
                    SET PER_NOMBRE = :PerNombre,
                        PER_APELLIDO = :PerApellido,
                        PER_DIRECCION = :PerDireccion,
                        PER_TELEFONO = :PerTelefono,
                        PER_EMAIL = :PerEmail
                    WHERE PER_ID = :PerId";

                int rows = db.Execute(sql, c);

                return rows > 0;
            }
        }

        /**
         * @brief Cuenta las facturas de un cliente.
         * @param perId PER_ID del cliente
         * @return Número de facturas
         */
        public int CountFacturas(decimal perId)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT COUNT(*) FROM TBL_FACTURA F
                               INNER JOIN TBL_CLIENTE C ON F.CLI_ID = C.CLI_ID
                               WHERE C.PER_ID = :Id";
                return db.ExecuteScalar<int>(sql, new { Id = perId });
            }
        }

        /**
         * @brief Elimina el cliente y su persona en una transacción.
         * @param id PER_ID del cliente
         * @return true si se elimino
         */
        public bool DeleteCascade(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        db.Execute("DELETE FROM TBL_CLIENTE WHERE PER_ID = :Id", new { Id = id }, tran);

                        int rows = db.Execute("DELETE FROM TBL_PERSONA WHERE PER_ID = :Id", new { Id = id }, tran);

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
         * @brief Busca clientes por nombre o apellido.
         * @param term Texto a buscar
         * @return Clientes que coinciden
         */
        public IEnumerable<Cliente> SearchByName(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ClienteSelectSql + @"
                                WHERE LOWER(P.PER_NOMBRE) LIKE LOWER(:Term) 
                                OR LOWER(P.PER_APELLIDO) LIKE LOWER(:Term)
                                ORDER BY P.PER_APELLIDO, P.PER_NOMBRE";
                return db.Query<Cliente>(sql, new { Term = $"%{term}%" });
            }
        }
    }
}
