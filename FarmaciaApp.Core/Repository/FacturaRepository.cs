using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FarmaciaApp.Core.Repositories
{
    public class FacturaRepository
    {
        // ✅ Consulta CORREGIDA
        private const string FacturaSelectSql = @"
            SELECT 
                F.FAC_NUM_FACTURA AS FacNumFactura,
                F.FAC_FECHA AS FacFecha,
                F.FAC_SUBTOTAL AS FacSubtotal,
                F.FAC_IVA AS FacIva,
                F.FAC_TOTAL AS FacTotal,
                F.CLI_ID AS CliId,
                F.VEN_ID AS VenId,
                F.PAG_ID AS PagId,
                P_CLI.PER_NOMBRE || ' ' || P_CLI.PER_APELLIDO AS ClienteNombre,
                P_VEN.PER_NOMBRE || ' ' || P_VEN.PER_APELLIDO AS VendedorNombre
            FROM 
                TBL_FACTURA F
            INNER JOIN TBL_CLIENTE C ON F.CLI_ID = C.CLI_ID
            INNER JOIN TBL_PERSONA P_CLI ON C.PER_ID = P_CLI.PER_ID
            INNER JOIN TBL_VENDEDOR V ON F.VEN_ID = V.VEN_ID
            INNER JOIN TBL_PERSONA P_VEN ON V.PER_ID = P_VEN.PER_ID
            ";

        public IEnumerable<Factura> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = FacturaSelectSql + " ORDER BY F.FAC_NUM_FACTURA DESC";
                return db.Query<Factura>(sql);
            }
        }

        public Factura GetById(decimal numero)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = FacturaSelectSql + " WHERE F.FAC_NUM_FACTURA = :Numero";
                return db.QueryFirstOrDefault<Factura>(sql, new { Numero = numero });
            }
        }

        public IEnumerable<Factura> Search(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = FacturaSelectSql + @"
                    WHERE LOWER(P_CLI.PER_NOMBRE) LIKE LOWER(:Term) 
                       OR LOWER(P_CLI.PER_APELLIDO) LIKE LOWER(:Term)
                       OR LOWER(P_VEN.PER_NOMBRE) LIKE LOWER(:Term) 
                       OR LOWER(P_VEN.PER_APELLIDO) LIKE LOWER(:Term)
                       OR TO_CHAR(F.FAC_NUM_FACTURA) LIKE :TermNumber
                    ORDER BY F.FAC_NUM_FACTURA DESC";

                return db.Query<Factura>(sql, new
                {
                    Term = $"%{term}%",
                    TermNumber = $"{term}%"
                });
            }
        }
    }
}