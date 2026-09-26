using System;

namespace FarmaciaApp.Core.Models
{
    public class Usuario
    {
        public const string RolAdministrador = "Administrador";
        public const string RolEmpleado = "Empleado";

        public decimal UsuId { get; set; }
        public string Login { get; set; }
        public string Rol { get; set; }
        public decimal? PerId { get; set; }
        public string NombrePersona { get; set; }
        public bool Activo { get; set; }
        public DateTime? UltimoIngreso { get; set; }

        // Credenciales (hash PBKDF2 y sal en Base64); nunca se muestran
        public string Hash { get; set; }
        public string Sal { get; set; }

        public bool EsAdmin => Rol == RolAdministrador;
        public string Nombre => string.IsNullOrWhiteSpace(NombrePersona) ? Login : NombrePersona.Trim();
        public string Estado => Activo ? "Activo" : "Inactivo";
    }
}
