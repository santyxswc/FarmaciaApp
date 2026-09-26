/**
 * @file UsuarioRepository.cs
 * @brief Acceso a datos de usuarios.
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
     * @brief Consultas y cambios de TBL_USUARIO.
     */
    public class UsuarioRepository
    {
        /** Consulta base de usuarios con el nombre de su persona. */
        private const string UsuarioSelectSql = @"
            SELECT U.USU_ID AS UsuId,
                   U.USU_LOGIN AS Login,
                   U.USU_HASH AS Hash,
                   U.USU_SAL AS Sal,
                   U.USU_ROL AS Rol,
                   U.PER_ID AS PerId,
                   P.PER_NOMBRE || ' ' || P.PER_APELLIDO AS NombrePersona,
                   U.USU_ACTIVO AS Activo,
                   U.USU_ULTIMO_INGRESO AS UltimoIngreso
            FROM TBL_USUARIO U
            LEFT JOIN TBL_PERSONA P ON P.PER_ID = U.PER_ID";

        /**
         * @brief Obtiene todos los usuarios ordenados por rol y login.
         * @return Usuarios
         */
        public IEnumerable<Usuario> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.Query<Usuario>(UsuarioSelectSql + " ORDER BY U.USU_ROL, U.USU_LOGIN");
            }
        }

        /**
         * @brief Busca un usuario.
         * @param id USU_ID
         * @return Usuario, o null si no existe
         */
        public Usuario GetById(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.QueryFirstOrDefault<Usuario>(UsuarioSelectSql + " WHERE U.USU_ID = :Id", new { Id = id });
            }
        }

        /**
         * @brief Busca un usuario por su login.
         * @param login Login en minúsculas
         * @return Usuario, o null si no existe
         */
        public Usuario GetByLogin(string login)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.QueryFirstOrDefault<Usuario>(UsuarioSelectSql + " WHERE U.USU_LOGIN = :Login", new { Login = login });
            }
        }

        /**
         * @brief Registra un usuario.
         * @param login Login en minúsculas
         * @param hash Hash de la contraseña
         * @param sal Sal del hash
         * @param rol Administrador o Empleado
         * @param perId Persona asociada, o null
         * @return USU_ID asignado
         */
        public decimal Insert(string login, string hash, string sal, string rol, decimal? perId)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Execute(@"INSERT INTO TBL_USUARIO (USU_LOGIN, USU_HASH, USU_SAL, USU_ROL, PER_ID)
                             VALUES (:Login, :Hash, :Sal, :Rol, :PerId)",
                           new { Login = login, Hash = hash, Sal = sal, Rol = rol, PerId = perId });
                return db.ExecuteScalar<decimal>("SELECT USU_ID FROM TBL_USUARIO WHERE USU_LOGIN = :Login", new { Login = login });
            }
        }

        /**
         * @brief Activa o desactiva un usuario.
         * @param id USU_ID
         * @param activo Nuevo estado
         */
        public void SetActivo(decimal id, bool activo)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Execute("UPDATE TBL_USUARIO SET USU_ACTIVO = :Activo WHERE USU_ID = :Id", new { Activo = activo ? 1 : 0, Id = id });
            }
        }

        /**
         * @brief Reemplaza la contraseña de un usuario.
         * @param id USU_ID
         * @param hash Hash nuevo
         * @param sal Sal nueva
         */
        public void SetClave(decimal id, string hash, string sal)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Execute("UPDATE TBL_USUARIO SET USU_HASH = :Hash, USU_SAL = :Sal WHERE USU_ID = :Id", new { Hash = hash, Sal = sal, Id = id });
            }
        }

        /**
         * @brief Guarda la fecha actual como último ingreso.
         * @param id USU_ID
         */
        public void RegistrarIngreso(decimal id)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Execute("UPDATE TBL_USUARIO SET USU_ULTIMO_INGRESO = SYSDATE WHERE USU_ID = :Id", new { Id = id });
            }
        }

        /**
         * @brief Cuenta los administradores activos.
         * @return Número de administradores activos
         */
        public int CountAdminsActivos()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM TBL_USUARIO WHERE USU_ROL = 'Administrador' AND USU_ACTIVO = 1");
            }
        }

        /**
         * @brief Cuenta las cuentas de empleado activas de una persona.
         * @param perId PER_ID
         * @return Número de cuentas
         */
        public int CountEmpleadosActivosPorPersona(int perId)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM TBL_USUARIO
                                               WHERE PER_ID = :Id AND USU_ROL = 'Empleado' AND USU_ACTIVO = 1", new { Id = perId });
            }
        }

        /**
         * @brief Cuenta las cuentas de usuario de una persona.
         * @param perId PER_ID
         * @return Número de cuentas
         */
        public int CountPorPersona(int perId)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM TBL_USUARIO WHERE PER_ID = :Id", new { Id = perId });
            }
        }

        /**
         * @brief Obtiene el VEN_ID de una persona.
         * @param perId PER_ID
         * @return VEN_ID, o null si la persona no es vendedor
         */
        public decimal? GetVenIdPorPersona(decimal perId)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.QueryFirstOrDefault<decimal?>("SELECT VEN_ID FROM TBL_VENDEDOR WHERE PER_ID = :Id", new { Id = perId });
            }
        }
    }
}
