using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class ClienteRepository
    {
        // Consulta base con JOIN de PERSONA y CLIENTE
        // Usamos alias (ej. PER_ID AS PerId) para que Dapper mapee automáticamente a Cliente.cs
        private const string ClienteSelectSql = @"
            SELECT 
                P.PER_ID AS PerId, 
                P.PER_NOMBRE AS PerNombre, 
                P.PER_APELLIDO AS PerApellido,
                P.PER_DIRECCION AS PerDireccion, 
                P.PER_TELEFONO AS PerTelefono, 
                P.PER_EMAIL AS PerEmail
                -- Si TBL_CLIENTE tiene campos extra, únelos aquí:
                -- , C.CLI_TIPO AS CliTipo 
            FROM 
                TBL_PERSONA P
            INNER JOIN 
                TBL_CLIENTE C ON P.PER_ID = C.PER_ID";

        public IEnumerable<Cliente> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                // Consulta para obtener todos los clientes
                string sql = ClienteSelectSql + " ORDER BY P.PER_APELLIDO, P.PER_NOMBRE";
                return db.Query<Cliente>(sql);
            }
        }

        public Cliente GetById(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ClienteSelectSql + " WHERE P.PER_ID = :Id";
                return db.QueryFirstOrDefault<Cliente>(sql, new { Id = id });
            }
        }

        public decimal Insert(Cliente c)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        // 1) Obtener el NEXTVAL de la secuencia para TBL_PERSONA
                        // Asumo que tu secuencia se llama SEQ_PERSONA
                        decimal newId = db.ExecuteScalar<decimal>("SELECT SEQ_PERSONA.NEXTVAL FROM DUAL", null, tran);

                        // 2) Insertar en TBL_PERSONA
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

                        // 3) Insertar en TBL_CLIENTE (usando el mismo ID)
                        string clienteSql = @"INSERT INTO TBL_CLIENTE (PER_ID) VALUES (:Id)";
                        db.Execute(clienteSql, new { Id = newId }, tran);

                        // 4) Confirmar la transacción
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

        public bool Update(Cliente c)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                // Solo se necesita actualizar TBL_PERSONA
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

        public bool DeleteCascade(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        // 1) Borrar de TBL_CLIENTE (tabla dependiente)
                        db.Execute("DELETE FROM TBL_CLIENTE WHERE PER_ID = :Id", new { Id = id }, tran);

                        // 2) Borrar de TBL_PERSONA (tabla principal)
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

        public IEnumerable<Cliente> SearchByName(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ClienteSelectSql + @"
                                WHERE LOWER(P.PER_NOMBRE) LIKE LOWER(:Term) 
                                OR LOWER(P.PER_APELLIDO) LIKE LOWER(:Term)
                                ORDER BY P.PER_APELLIDO, P.PER_NOMBRE";

                // Dapper maneja los parámetros correctamente para Oracle usando :Term
                return db.Query<Cliente>(sql, new { Term = $"%{term}%" });
            }
        }
    }
}