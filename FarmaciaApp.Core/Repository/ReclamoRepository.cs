/**
 * @file ReclamoRepository.cs
 * @brief Acceso a datos de reclamos.
 * @author Santiago Caicedo
 */
using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    /**
     * @brief Consultas y cambios de TBL_RECLAMO.
     */
    public class ReclamoRepository
    {
        /** Consulta base de reclamos con el nombre del cliente de la factura. */
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

        /**
         * @brief Obtiene todos los reclamos, del más reciente al más antiguo.
         * @return Reclamos
         */
        public IEnumerable<Reclamo> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ReclamoSelectSql + " ORDER BY R.REC_FECHA DESC";
                return db.Query<Reclamo>(sql);
            }
        }

        /**
         * @brief Busca un reclamo.
         * @param id Número del reclamo
         * @return Reclamo, o null si no existe
         */
        public Reclamo GetById(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = ReclamoSelectSql + " WHERE R.REC_ID_RECLAMO = :Id";
                return db.QueryFirstOrDefault<Reclamo>(sql, new { Id = id });
            }
        }

        /**
         * @brief Registra un reclamo con la fecha actual.
         * @param r Datos del reclamo (sin estado queda Pendiente)
         * @return Número asignado por SEQ_RECLAMO
         */
        public decimal Insert(Reclamo r)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                decimal newId = db.ExecuteScalar<decimal>("SELECT SEQ_RECLAMO.NEXTVAL FROM DUAL");
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

        /**
         * @brief Actualiza la descripción y el estado de un reclamo.
         * @param r Reclamo con los datos nuevos
         * @return true si se actualizo
         */
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

        /**
         * @brief Elimina un reclamo y sus reintegros en una transacción.
         * @param id Número del reclamo
         * @return true si se elimino
         */
        public bool Delete(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        db.Execute(
    "DELETE FROM TBL_REINTEGRO WHERE REC_ID_RECLAMO = :Id",
    new { Id = id },
    tran
);

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

        /**
         * @brief Busca reclamos por descripción, estado, cliente o número de factura.
         * @param term Texto a buscar
         * @return Reclamos que coinciden
         */
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

        /**
         * @brief Obtiene los reclamos con un estado.
         * @param estado Estado buscado
         * @return Reclamos en ese estado
         */
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

        /**
         * @brief Obtiene los reclamos de una factura.
         * @param facNumFactura Número de factura
         * @return Reclamos de la factura
         */
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
