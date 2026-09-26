/**
 * @file Usuario.cs
 * @brief Modelo de usuario del sistema.
 * @author Santiago Caicedo
 */
using System;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Cuenta que inicia sesión en la aplicación (TBL_USUARIO).
     */
    public class Usuario
    {
        /** Rol con acceso completo y a la sección de administración. */
        public const string RolAdministrador = "Administrador";
        /** Rol de vendedor de la farmacia. */
        public const string RolEmpleado = "Empleado";

        /** Identificador (USU_ID). */
        public decimal UsuId { get; set; }
        /** Nombre de usuario, en minúsculas. */
        public string Login { get; set; }
        /** Administrador o Empleado. */
        public string Rol { get; set; }
        /** Persona asociada (obligatoria para empleados). */
        public decimal? PerId { get; set; }
        /** Nombre completo de la persona asociada. */
        public string NombrePersona { get; set; }
        /** Indica si puede iniciar sesión. */
        public bool Activo { get; set; }
        /** Fecha del último inicio de sesión. */
        public DateTime? UltimoIngreso { get; set; }

        /** Hash PBKDF2 de la contraseña (Base64). Nunca se muestra. */
        public string Hash { get; set; }
        /** Sal del hash (Base64). */
        public string Sal { get; set; }

        /** Indica si tiene rol de administrador. */
        public bool EsAdmin => Rol == RolAdministrador;
        /** Nombre para mostrar: el de la persona o, si no tiene, el login. */
        public string Nombre => string.IsNullOrWhiteSpace(NombrePersona) ? Login : NombrePersona.Trim();
        /** Activo o Inactivo. */
        public string Estado => Activo ? "Activo" : "Inactivo";
    }
}
