using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class ProductoService
    {
        private readonly ProductoRepository _repo;

        public ProductoService()
        {
            _repo = new ProductoRepository();
        }

        public IEnumerable<Producto> ObtenerProductos() => _repo.GetAll();

        public Producto ObtenerPorId(int id) => _repo.GetById(id);

        public int CrearProducto(Producto p)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (p.ProPrecio <= 0)
                throw new ArgumentException("El precio debe ser mayor que cero.");
            if (p.ProStock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            return _repo.Insert(p);
        }

        public bool ActualizarProducto(Producto p)
        {
            if (p.ProId <= 0)
                throw new ArgumentException("Id de producto inválido.");
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (p.ProPrecio <= 0)
                throw new ArgumentException("El precio debe ser mayor que cero.");
            if (p.ProStock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            return _repo.Update(p);
        }

        public bool EliminarProducto(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            // Delete en cascada desde el repositorio
            return _repo.DeleteCascade(id);
        }

        public IEnumerable<Producto> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}