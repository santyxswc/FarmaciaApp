/**
 * @file UsuarioService.cs
 * @brief Inicio de sesión y administración de usuarios.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using FarmaciaApp.Core.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Autenticación, turnos y cuentas de usuario.
     */
    public class UsuarioService
    {
        /** Roles que se pueden asignar. */
        public static readonly string[] Roles = { Usuario.RolEmpleado, Usuario.RolAdministrador };

        /** Largo mínimo de una contraseña. */
        private const int LargoMinimoClave = 6;
        /** Formato válido de un login: minúsculas, números, punto, guion y guion bajo. */
        private static readonly Regex LoginValido = new Regex(@"^[a-z0-9._-]{3,30}$");

        private readonly UsuarioRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public UsuarioService()
        {
            _repo = new UsuarioRepository();
        }

        /**
         * @brief Verifica las credenciales e inicia la sesión.
         * @param login Usuario (no distingue mayúsculas ni espacios en los extremos)
         * @param clave Contraseña
         * @return Usuario autenticado, o null si las credenciales no son correctas
         * @exception InvalidOperationException Si el usuario está desactivado
         *
         * Registra el ingreso, o el intento fallido, en los movimientos.
         */
        public Usuario IniciarSesion(string login, string clave)
        {
            login = NormalizarLogin(login);
            var usuario = string.IsNullOrEmpty(login) ? null : _repo.GetByLogin(login);

            if (usuario == null || !Claves.Verificar(clave, usuario.Hash, usuario.Sal))
            {
                Auditoria.Registrar("Inicio de sesión fallido", $"Usuario: {login}");
                return null;
            }

            if (!usuario.Activo)
            {
                Auditoria.Registrar("Inicio de sesión rechazado", $"Usuario desactivado: {login}");
                throw new InvalidOperationException("Este usuario está desactivado. Consulta con el administrador.");
            }

            _repo.RegistrarIngreso(usuario.UsuId);
            decimal? venId = usuario.PerId.HasValue ? _repo.GetVenIdPorPersona(usuario.PerId.Value) : null;
            Sesion.Iniciar(usuario, venId);
            Auditoria.Registrar("Inicio de sesión", $"Turno iniciado ({usuario.Rol})");
            return usuario;
        }

        /**
         * @brief Cierra la sesión y registra la duracion del turno.
         * @param motivo Motivo opcional ("ventana cerrada")
         */
        public void CerrarSesion(string motivo = null)
        {
            if (!Sesion.Activa) return;

            var duracion = DateTime.Now - Sesion.InicioTurno;
            Auditoria.Registrar("Cierre de sesión",
                $"Turno de {(int)duracion.TotalHours} h {duracion.Minutes} min{(motivo == null ? "" : " · " + motivo)}");
            Sesion.Cerrar();
        }

        /**
         * @brief Obtiene todos los usuarios. Solo administrador.
         * @return Usuarios
         */
        public IEnumerable<Usuario> ObtenerUsuarios()
        {
            Sesion.ExigirAdmin("ver los usuarios");
            return _repo.GetAll();
        }

        /**
         * @brief Crea una cuenta. Solo administrador.
         * @param login Usuario
         * @param clave Contraseña
         * @param confirmacion Confirmación de la contraseña
         * @param rol Administrador o Empleado
         * @param perId Persona asociada; obligatoria para empleados
         * @return USU_ID asignado
         * @exception ArgumentException Si los datos no son válidos
         * @exception InvalidOperationException Si el login ya existe
         *
         * La persona de un empleado queda marcada como vendedor.
         */
        public decimal CrearUsuario(string login, string clave, string confirmacion, string rol, decimal? perId)
        {
            Sesion.ExigirAdmin("crear usuarios");

            login = NormalizarLogin(login);
            if (!LoginValido.IsMatch(login ?? ""))
                throw new ArgumentException("El usuario debe tener entre 3 y 30 caracteres: letras minúsculas, números, punto, guion o guion bajo.");
            if (!Roles.Contains(rol))
                throw new ArgumentException("Selecciona un rol.");
            if (rol == Usuario.RolEmpleado && perId == null)
                throw new ArgumentException("Un empleado debe estar asociado a una persona (quedará como vendedor).");
            ValidarClave(clave, confirmacion);
            if (_repo.GetByLogin(login) != null)
                throw new InvalidOperationException($"El usuario '{login}' ya existe.");

            if (rol == Usuario.RolEmpleado)
                new PersonaService().AsignarVendedor((int)perId.Value, true);

            var (hash, sal) = Claves.Crear(clave);
            decimal id = _repo.Insert(login, hash, sal, rol, perId);
            Auditoria.Registrar("Usuario creado", $"{login} ({rol})");
            return id;
        }

        /**
         * @brief Activa o desactiva una cuenta. Solo administrador.
         * @param usuId USU_ID
         * @param activo Nuevo estado
         * @exception InvalidOperationException Si se desactiva la propia cuenta o el último administrador activo
         */
        public void CambiarEstado(decimal usuId, bool activo)
        {
            Sesion.ExigirAdmin("activar o desactivar usuarios");

            var usuario = _repo.GetById(usuId) ?? throw new InvalidOperationException("El usuario ya no existe.");
            if (!activo)
            {
                if (Sesion.Usuario?.UsuId == usuId)
                    throw new InvalidOperationException("No puedes desactivar tu propio usuario.");
                if (usuario.EsAdmin && usuario.Activo && _repo.CountAdminsActivos() <= 1)
                    throw new InvalidOperationException("Debe quedar al menos un administrador activo.");
            }

            _repo.SetActivo(usuId, activo);
            Auditoria.Registrar(activo ? "Usuario activado" : "Usuario desactivado", usuario.Login);
        }

        /**
         * @brief Asigna una contraseña nueva a otra cuenta. Solo administrador.
         * @param usuId USU_ID
         * @param nueva Contraseña nueva
         * @param confirmacion Confirmación
         */
        public void RestablecerClave(decimal usuId, string nueva, string confirmacion)
        {
            Sesion.ExigirAdmin("restablecer contraseñas");
            ValidarClave(nueva, confirmacion);

            var usuario = _repo.GetById(usuId) ?? throw new InvalidOperationException("El usuario ya no existe.");
            var (hash, sal) = Claves.Crear(nueva);
            _repo.SetClave(usuId, hash, sal);
            Auditoria.Registrar("Contraseña restablecida", usuario.Login);
        }

        /**
         * @brief Cambia la contraseña del usuario de la sesión.
         * @param actual Contraseña actual
         * @param nueva Contraseña nueva
         * @param confirmacion Confirmación
         * @exception InvalidOperationException Si la contraseña actual no es correcta
         */
        public void CambiarMiClave(string actual, string nueva, string confirmacion)
        {
            if (!Sesion.Activa)
                throw new InvalidOperationException("No hay una sesión iniciada.");

            var usuario = _repo.GetById(Sesion.Usuario.UsuId);
            if (!Claves.Verificar(actual, usuario.Hash, usuario.Sal))
                throw new InvalidOperationException("La contraseña actual no es correcta.");
            ValidarClave(nueva, confirmacion);
            if (nueva == actual)
                throw new ArgumentException("La nueva contraseña debe ser diferente de la actual.");

            var (hash, sal) = Claves.Crear(nueva);
            _repo.SetClave(usuario.UsuId, hash, sal);
            Auditoria.Registrar("Cambio de contraseña", usuario.Login);
        }

        /**
         * @brief Verifica el largo y la confirmación de una contraseña.
         * @param clave Contraseña
         * @param confirmacion Confirmación
         * @exception ArgumentException Si es muy corta o no coincide
         */
        private static void ValidarClave(string clave, string confirmacion)
        {
            if (string.IsNullOrEmpty(clave) || clave.Length < LargoMinimoClave)
                throw new ArgumentException($"La contraseña debe tener al menos {LargoMinimoClave} caracteres.");
            if (clave != confirmacion)
                throw new ArgumentException("Las contraseñas no coinciden.");
        }

        /**
         * @brief Quita espacios y pasa el login a minúsculas.
         * @param login Login escrito
         * @return Login normalizado
         */
        private static string NormalizarLogin(string login) => login?.Trim().ToLowerInvariant();
    }
}
