/**
 * @file PersonaRepository.cs
 * @brief Acceso a datos de personas.
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
     * @brief Consultas y cambios de TBL_PERSONA y del rol de vendedor.
     */
    public class PersonaRepository
    {
        /** Consulta base de personas con sus roles de cliente y vendedor. */
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

        /**
         * @brief Obtiene todas las personas ordenadas por nombre.
         * @return Personas con sus roles
         */
        public IEnumerable<Persona> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = PersonaSelectSql + " ORDER BY P.PER_NOMBRE, P.PER_APELLIDO";
                return db.Query<Persona>(sql);
            }
        }

        /**
         * @brief Busca una persona.
         * @param id PER_ID
         * @return Persona, o null si no existe
         */
        public Persona GetById(int id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = PersonaSelectSql + " WHERE P.PER_ID = :Id";
                return db.QueryFirstOrDefault<Persona>(sql, new { Id = id });
            }
        }

        /**
         * @brief Registra una persona.
         * @param p Datos de la persona
         * @return PER_ID asignado por SEQ_PERSONA (la misma secuencia que usan los clientes)
         */
        public int Insert(Persona p)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
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

        /**
         * @brief Actualiza los datos personales.
         * @param p Persona con los datos nuevos
         * @return true si se actualizo
         */
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

        /**
         * @brief Cuenta las facturas donde la persona es cliente o vendedor.
         * @param id PER_ID
         * @return Número de facturas
         */
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

        /**
         * @brief Cuenta las facturas que la persona registró como vendedor.
         * @param id PER_ID
         * @return Número de facturas
         */
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

        /**
         * @brief Agrega o quita a la persona de TBL_VENDEDOR.
         * @param id PER_ID
         * @param esVendedor true para agregarla, false para quitarla
         */
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

        /**
         * @brief Elimina la persona y sus registros de cliente y vendedor en una transacción.
         * @param id PER_ID
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
                        db.Execute("DELETE FROM TBL_CLIENTE WHERE PER_ID = :Id", new { Id = id }, tran);
                        db.Execute("DELETE FROM TBL_VENDEDOR WHERE PER_ID = :Id", new { Id = id }, tran);

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
         * @brief Busca personas por nombre, apellido o email.
         * @param term Texto a buscar
         * @return Personas que coinciden
         */
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
