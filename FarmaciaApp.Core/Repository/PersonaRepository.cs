using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    public class PersonaRepository
    {
        public IEnumerable<Persona> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT 
                                   PER_ID AS PerId,
                                   PER_NOMBRE AS PerNombre,
                                   PER_APELLIDO AS PerApellido,
                                   PER_DIRECCION AS PerDireccion,
                                   PER_TELEFONO AS PerTelefono,
                                   PER_EMAIL AS PerEmail
                               FROM TBL_PERSONA
                               ORDER BY PER_NOMBRE, PER_APELLIDO";
                return db.Query<Persona>(sql);
            }
        }

        public Persona GetById(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"SELECT PER_ID AS PerId, PER_NOMBRE AS PerNombre, 
                                      PER_APELLIDO AS PerApellido, PER_DIRECCION AS PerDireccion,
                                      PER_TELEFONO AS PerTelefono, PER_EMAIL AS PerEmail
                               FROM TBL_PERSONA
                               WHERE PER_ID = :Id";
                return db.QueryFirstOrDefault<Persona>(sql, new { Id = id });
            }
        }

        public int Insert(Persona p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                // Obtener el siguiente ID (asumiendo secuencia SEQ_PERSONA)
                int newId = db.ExecuteScalar<int>("SELECT NVL(MAX(PER_ID), 0) + 1 FROM TBL_PERSONA");

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
                string sql = @"SELECT PER_ID AS PerId, PER_NOMBRE AS PerNombre, 
                                      PER_APELLIDO AS PerApellido, PER_DIRECCION AS PerDireccion,
                                      PER_TELEFONO AS PerTelefono, PER_EMAIL AS PerEmail
                               FROM TBL_PERSONA
                               WHERE LOWER(PER_NOMBRE) LIKE LOWER(:Term)
                                  OR LOWER(PER_APELLIDO) LIKE LOWER(:Term)
                                  OR LOWER(PER_EMAIL) LIKE LOWER(:Term)";
                return db.Query<Persona>(sql, new { Term = $"%{term}%" });
            }
        }
    }
}