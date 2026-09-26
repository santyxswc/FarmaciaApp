using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class PersonaRepository
    {
        private const string PersonaSelectSql = @"
            SELECT P.PER_ID AS PerId,
                   P.PER_NOMBRE AS PerNombre,
                   P.PER_APELLIDO AS PerApellido,
                   P.PER_DIRECCION AS PerDireccion,
                   P.PER_TELEFONO AS PerTelefono,
                   P.PER_EMAIL AS PerEmail,
                   CASE WHEN EXISTS (SELECT 1 FROM TBL_CLIENTE C WHERE C.PER_ID = P.PER_ID) THEN 1 ELSE 0 END AS EsCliente,
                   CASE WHEN EXISTS (SELECT 1 FROM TBL_VENDEDOR V WHERE V.PER_ID = P.PER_ID) THEN 1 ELSE 0 END AS EsVendedor
            FROM TBL_PERSONA P";

        public IEnumerable<Persona> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = PersonaSelectSql + " ORDER BY P.PER_NOMBRE, P.PER_APELLIDO";
                return db.Query<Persona>(sql);
            }
        }

        public Persona GetById(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = PersonaSelectSql + " WHERE P.PER_ID = :Id";
                return db.QueryFirstOrDefault<Persona>(sql, new { Id = id });
            }
        }

        public int Insert(Persona p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                // Misma secuencia que usa ClienteRepository, para que los IDs no choquen
                int newId = db.ExecuteScalar<int>("SELECT SEQ_PERSONA.NEXTVAL FROM DUAL");

                string sql = @"
                    INSERT INTO TBL_PERSONA 
                        (PER_ID, PER_NOMBRE, PER_APELLIDO, PER_DIRECCION, PER_TELEFONO, PER_EMAIL)
                    VALUES 
                        (:Id, :PerNombre, :PerApellido, :PerDireccion, :PerTelefono, :PerEmail)";

                db.Execute(sql, new
                {
                    Id = newId,
                    p.PerNombre,
                    p.PerApellido,
                    p.PerDireccion,
                    p.PerTelefono,
                    p.PerEmail
                });

                return newId;
            }
        }

        public bool Update(Persona p)
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

                int rows = db.Execute(sql, new
                {
                    p.PerNombre,
                    p.PerApellido,
                    p.PerDireccion,
                    p.PerTelefono,
                    p.PerEmail,
                    p.PerId
                });

                return rows > 0;
            }
        }

        // Facturas donde la persona aparece como cliente o como vendedor
        public int CountFacturas(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT COUNT(*) FROM TBL_FACTURA F
                               WHERE F.CLI_ID IN (SELECT CLI_ID FROM TBL_CLIENTE WHERE PER_ID = :Id)
                                  OR F.VEN_ID IN (SELECT VEN_ID FROM TBL_VENDEDOR WHERE PER_ID = :Id)";
                return db.ExecuteScalar<int>(sql, new { Id = id });
            }
        }

        public int CountFacturasComoVendedor(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT COUNT(*) FROM TBL_FACTURA F
                               INNER JOIN TBL_VENDEDOR V ON F.VEN_ID = V.VEN_ID
                               WHERE V.PER_ID = :Id";
                return db.ExecuteScalar<int>(sql, new { Id = id });
            }
        }

        // Agrega o quita a la persona de TBL_VENDEDOR (VEN_ID lo genera la identidad)
        public void SetVendedor(int id, bool esVendedor)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                if (esVendedor)
                    db.Execute(@"INSERT INTO TBL_VENDEDOR (PER_ID)
                                 SELECT :Id FROM DUAL
                                 WHERE NOT EXISTS (SELECT 1 FROM TBL_VENDEDOR WHERE PER_ID = :Id)", new { Id = id });
                else
                    db.Execute("DELETE FROM TBL_VENDEDOR WHERE PER_ID = :Id", new { Id = id });
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
                        // Eliminar relaciones en cascada
                        db.Execute("DELETE FROM TBL_CLIENTE WHERE PER_ID = :Id", new { Id = id }, tran);
                        db.Execute("DELETE FROM TBL_VENDEDOR WHERE PER_ID = :Id", new { Id = id }, tran);

                        // Eliminar persona
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

        public IEnumerable<Persona> SearchByName(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = PersonaSelectSql + @"
                               WHERE LOWER(P.PER_NOMBRE) LIKE LOWER(:Term)
                                  OR LOWER(P.PER_APELLIDO) LIKE LOWER(:Term)
                                  OR LOWER(P.PER_EMAIL) LIKE LOWER(:Term)
                               ORDER BY P.PER_NOMBRE, P.PER_APELLIDO";
                return db.Query<Persona>(sql, new { Term = $"%{term}%" });
            }
        }
    }
}