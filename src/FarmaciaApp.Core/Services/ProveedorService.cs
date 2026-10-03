/**
 * @file ProveedorService.cs
 * @brief Reglas de negocio de los proveedores.
 * @author Santiago Caicedo
 */
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Text;
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de los proveedores.
     */
    public class ProveedorService
    {
        /** Acceso a datos de proveedores. */
        private readonly IProveedorRepository _repo;
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;
        /** Registro de movimientos. */
        private readonly IAuditoria _auditoria;

        /**
         * @brief Crea el servicio con sus dependencias.
         * @param repo Acceso a datos
         * @param sesion Usuario del turno (permisos)
         * @param auditoria Registro de movimientos
         */
        public ProveedorService(IProveedorRepository repo, ISesionUsuario sesion, IAuditoria auditoria)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
            _auditoria = auditoria ?? throw new ArgumentNullException(nameof(auditoria));
        }

        /**
         * @brief Obtiene todos los registros.
         * @return Proveedores
         */
        public IEnumerable<Proveedor> ObtenerProveedores() => _repo.GetAll();

        /**
         * @brief Busca un registro.
         * @param id Identificador
         * @return Registro, o null
         */
        public Proveedor ObtenerPorId(int id) => _repo.GetById(id);

        /**
         * @brief Valida y registra. Solo administrador.
         * @param p Datos a registrar
         * @return Identificador asignado
         * @exception ArgumentException Si faltan datos o no son válidos
         */
        public int CrearProveedor(Proveedor p)
        {
            _sesion.ExigirAdmin("crear proveedores");
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");

            int id = _repo.Insert(p);
            _auditoria.Registrar("Proveedor creado", p.ProNombre);
            return id;
        }

        /**
         * @brief Valida y actualiza. Solo administrador.
         * @param p Registro con los datos nuevos
         * @return true si se actualizo
         * @exception ArgumentException Si faltan datos o no son válidos
         */
        public bool ActualizarProveedor(Proveedor p)
        {
            _sesion.ExigirAdmin("modificar proveedores");
            if (p.ProId <= 0)
                throw new ArgumentException("Id de proveedor inválido.");
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");

            bool ok = _repo.Update(p);
            if (ok)
                _auditoria.Registrar("Proveedor modificado", p.ProNombre);
            return ok;
        }

        /**
         * @brief Elimina el registro y sus relaciones con productos. Solo administrador.
         * @param id Identificador
         * @return true si se elimino
         */
        public bool EliminarProveedor(int id)
        {
            _sesion.ExigirAdmin("eliminar proveedores");
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            var registro = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                _auditoria.Registrar("Proveedor eliminado", registro?.ProNombre);
            return ok;
        }

        /**
         * @brief Busca proveedores por nombre o contacto.
         * @param termino Texto a buscar
         * @return Proveedores que coinciden
         */
        public IEnumerable<Proveedor> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}
