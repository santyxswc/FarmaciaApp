/**
 * @file IUsuarioRepository.cs
 * @brief Contrato de acceso a datos de usuario.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Operaciones de persistencia de usuario que usan los servicios.
     *
     * La implementación con Oracle y Dapper está en FarmaciaApp.Infrastructure.
     */
    public interface IUsuarioRepository
    {
        /**
         * @brief Obtiene todos los usuarios ordenados por rol y login.
         * @return Usuarios
         */
        IEnumerable<Usuario> GetAll();

        /**
         * @brief Busca un usuario.
         * @param id USU_ID
         * @return Usuario, o null si no existe
         */
        Usuario GetById(decimal id);

        /**
         * @brief Busca un usuario por su login.
         * @param login Login en minúsculas
         * @return Usuario, o null si no existe
         */
        Usuario GetByLogin(string login);

        /**
         * @brief Registra un usuario.
         * @param login Login en minúsculas
         * @param hash Hash de la contraseña
         * @param sal Sal del hash
         * @param rol Administrador o Empleado
         * @param perId Persona asociada, o null
         * @return USU_ID asignado
         */
        decimal Insert(string login, string hash, string sal, string rol, decimal? perId);

        /**
         * @brief Activa o desactiva un usuario.
         * @param id USU_ID
         * @param activo Nuevo estado
         */
        void SetActivo(decimal id, bool activo);

        /**
         * @brief Reemplaza la contraseña de un usuario.
         * @param id USU_ID
         * @param hash Hash nuevo
         * @param sal Sal nueva
         */
        void SetClave(decimal id, string hash, string sal);

        /**
         * @brief Guarda la fecha actual como último ingreso.
         * @param id USU_ID
         */
        void RegistrarIngreso(decimal id);

        /**
         * @brief Cuenta los administradores activos.
         * @return Número de administradores activos
         */
        int CountAdminsActivos();

        /**
         * @brief Cuenta las cuentas de empleado activas de una persona.
         * @param perId PER_ID
         * @return Número de cuentas
         */
        int CountEmpleadosActivosPorPersona(int perId);

        /**
         * @brief Cuenta las cuentas de usuario de una persona.
         * @param perId PER_ID
         * @return Número de cuentas
         */
        int CountPorPersona(int perId);

        /**
         * @brief Obtiene el VEN_ID de una persona.
         * @param perId PER_ID
         * @return VEN_ID, o null si la persona no es vendedor
         */
        decimal? GetVenIdPorPersona(decimal perId);
    }
}
