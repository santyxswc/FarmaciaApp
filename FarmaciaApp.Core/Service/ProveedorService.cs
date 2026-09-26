/**
 * @file ProveedorService.cs
 * @brief Reglas de negocio de los proveedores.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de los proveedores.
     */
    public class ProveedorService
    {
        private readonly ProveedorRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public ProveedorService()
        {
            _repo = new ProveedorRepository();
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
            Sesion.ExigirAdmin("crear proveedores");
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");

            int id = _repo.Insert(p);
            Auditoria.Registrar("Proveedor creado", p.ProNombre);
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
            Sesion.ExigirAdmin("modificar proveedores");
            if (p.ProId <= 0)
                throw new ArgumentException("Id de proveedor inválido.");
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");

            bool ok = _repo.Update(p);
            if (ok)
                Auditoria.Registrar("Proveedor modificado", p.ProNombre);
            return ok;
        }

        /**
         * @brief Elimina el registro y sus relaciones con productos. Solo administrador.
         * @param id Identificador
         * @return true si se elimino
         */
        public bool EliminarProveedor(int id)
        {
            Sesion.ExigirAdmin("eliminar proveedores");
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            var registro = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                Auditoria.Registrar("Proveedor eliminado", registro?.ProNombre);
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
