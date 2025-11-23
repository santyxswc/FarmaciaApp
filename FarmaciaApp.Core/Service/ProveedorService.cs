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
            // Validaciones
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");

            return _repo.Insert(p);
        }

        public bool ActualizarProveedor(Proveedor p)
        {
            if (p.ProId <= 0)
                throw new ArgumentException("Id de proveedor inválido.");
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");

            return _repo.Update(p);
        }

        public bool EliminarProveedor(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            return _repo.DeleteCascade(id);
        }

        public IEnumerable<Proveedor> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}