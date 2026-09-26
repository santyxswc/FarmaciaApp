using System;
using System.Collections.Generic;
using System.Text;

using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class ProveedorService
    {
        private readonly ProveedorRepository _repo;

        public ProveedorService()
        {
            _repo = new ProveedorRepository();
        }

        public IEnumerable<Proveedor> ObtenerProveedores() => _repo.GetAll();

        public Proveedor ObtenerPorId(int id) => _repo.GetById(id);

        public int CrearProveedor(Proveedor p)
        {
            Sesion.ExigirAdmin("crear proveedores");
                        if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");

            int id = _repo.Insert(p);
            Auditoria.Registrar("Proveedor creado", p.ProNombre);
            return id;
        }

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

        public IEnumerable<Proveedor> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}