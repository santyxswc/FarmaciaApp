using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class ReclamoRepository
    {
        // ✅ Consulta base completa con REC_ESTADO
        private const string ReclamoSelectSql = @"
            SELECT 
                R.REC_ID_RECLAMO AS RecId, 
                R.REC_FECHA AS RecFecha, 
                R.REC_DESCRIPCION AS RecDescripcion,
                R.REC_ESTADO AS RecEstado,
                R.FAC_NUM_FACTURA AS FacNumFactura,
                P.PER_NOMBRE || ' ' || P.PER_APELLIDO AS ClienteNombre 
            FROM 
                TBL_RECLAMO R
            INNER JOIN TBL_FACTURA F ON R.FAC_NUM_FACTURA = F.FAC_NUM_FACTURA
            INNER JOIN TBL_CLIENTE C ON F.CLI_ID = C.CLI_ID
            INNER JOIN TBL_PERSONA P ON C.PER_ID = P.PER_ID
            ";

        /// <summary>
        /// Obtiene todos los reclamos ordenados por fecha descendente
        /// </summary>
        public IEnumerable<Reclamo> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ReclamoSelectSql + " ORDER BY R.REC_FECHA DESC";
                return db.Query<Reclamo>(sql);
            }
        }

        /// <summary>
        /// Obtiene un reclamo específico por su ID
        /// </summary>
        public Reclamo GetById(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ReclamoSelectSql + " WHERE R.REC_ID_RECLAMO = :Id";
                return db.QueryFirstOrDefault<Reclamo>(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Inserta un nuevo reclamo y retorna el ID generado
        /// </summary>
        public decimal Insert(Reclamo r)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                // 1) Obtener el siguiente valor de la secuencia
                decimal newId = db.ExecuteScalar<decimal>("SELECT SEQ_RECLAMO.NEXTVAL FROM DUAL");

                // 2) Insertar el reclamo con todos los campos
                string sql = @"
                    INSERT INTO TBL_RECLAMO 
                        (REC_ID_RECLAMO, FAC_NUM_FACTURA, REC_FECHA, REC_DESCRIPCION, REC_ESTADO)
                    VALUES 
                        (:Id, :FacNumFactura, SYSDATE, :RecDescripcion, :RecEstado)";

                db.Execute(sql, new
                {
                    Id = newId,
                    r.FacNumFactura,
                    r.RecDescripcion,
                    RecEstado = string.IsNullOrWhiteSpace(r.RecEstado) ? "Pendiente" : r.RecEstado
                });

                return newId;
            }
        }

        /// <summary>
        /// Actualiza un reclamo existente (descripción y estado)
        /// </summary>
        public bool Update(Reclamo r)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    UPDATE TBL_RECLAMO
                    SET REC_DESCRIPCION = :RecDescripcion,
                        REC_ESTADO = :RecEstado
                    WHERE REC_ID_RECLAMO = :RecId";

                int rows = db.Execute(sql, new
                {
                    r.RecDescripcion,
                    RecEstado = string.IsNullOrWhiteSpace(r.RecEstado) ? "Pendiente" : r.RecEstado,
                    r.RecId
                });

                return rows > 0;
            }
        }

        /// <summary>
        /// Elimina un reclamo y sus reintegros asociados en cascada
        /// </summary>
        public bool Delete(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        // 1. Eliminar reintegros asociados primero (evita errores de FK)
                        db.Execute(
                            "DELETE FROM TBL_REINTEGRO WHERE REC_ID_RECLAMO = :Id",
                            new { Id = id },
                            tran
                        );

                        // 2. Eliminar el reclamo
                        int rows = db.Execute(
                            "DELETE FROM TBL_RECLAMO WHERE REC_ID_RECLAMO = :Id",
                            new { Id = id },
                            tran
                        );

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

        /// <summary>
        /// Busca reclamos por descripción, estado, nombre del cliente o número de factura
        /// </summary>
        public IEnumerable<Reclamo> Search(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ReclamoSelectSql + @"
                    WHERE LOWER(R.REC_DESCRIPCION) LIKE LOWER(:Term) 
                       OR LOWER(R.REC_ESTADO) LIKE LOWER(:Term)
                       OR LOWER(P.PER_NOMBRE) LIKE LOWER(:Term)
                       OR LOWER(P.PER_APELLIDO) LIKE LOWER(:Term)
                       OR TO_CHAR(R.FAC_NUM_FACTURA) LIKE :TermNumber
                    ORDER BY R.REC_FECHA DESC";

                return db.Query<Reclamo>(sql, new
                {
                    Term = $"%{term}%",
                    TermNumber = $"{term}%"
                });
            }
        }

        /// <summary>
        /// Obtiene reclamos filtrados por estado específico
        /// </summary>
        public IEnumerable<Reclamo> GetByEstado(string estado)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ReclamoSelectSql + @"
                    WHERE LOWER(R.REC_ESTADO) = LOWER(:Estado)
                    ORDER BY R.REC_FECHA DESC";

                return db.Query<Reclamo>(sql, new { Estado = estado });
            }
        }

        /// <summary>
        /// Obtiene reclamos asociados a una factura específica
        /// </summary>
        public IEnumerable<Reclamo> GetByFactura(decimal facNumFactura)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ReclamoSelectSql + @"
                    WHERE R.FAC_NUM_FACTURA = :FacNumFactura
                    ORDER BY R.REC_FECHA DESC";

                return db.Query<Reclamo>(sql, new { FacNumFactura = facNumFactura });
            }
        }
    }
}