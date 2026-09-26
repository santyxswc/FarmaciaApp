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
            Sesion.ExigirAdmin("crear productos");
                        if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (p.ProPrecio <= 0)
                throw new ArgumentException("El precio debe ser mayor que cero.");
            if (p.ProStock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            int id = _repo.Insert(p);
            Auditoria.Registrar("Producto creado", $"{p.ProNombre} · precio {p.ProPrecio:C0} · stock {p.ProStock}");
            return id;
        }

        public bool ActualizarProducto(Producto p)
        {
            Sesion.ExigirAdmin("modificar productos");
            if (p.ProId <= 0)
                throw new ArgumentException("Id de producto inválido.");
            if (string.IsNullOrWhiteSpace(p.ProNombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (p.ProPrecio <= 0)
                throw new ArgumentException("El precio debe ser mayor que cero.");
            if (p.ProStock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            var anterior = _repo.GetById(p.ProId);
            bool ok = _repo.Update(p);
            if (ok)
                Auditoria.Registrar("Producto modificado", $"{p.ProNombre}{DescribirCambios(anterior, p)}");
            return ok;
        }

        public bool EliminarProducto(int id)
        {
            Sesion.ExigirAdmin("eliminar productos");
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            // Las facturas son registros historicos: un producto vendido no se puede borrar
            int ventas = _repo.CountVentas(id);
            if (ventas > 0)
                throw new InvalidOperationException($"No se puede eliminar el producto porque aparece en {ventas} factura(s).");

            var producto = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                Auditoria.Registrar("Producto eliminado", producto?.ProNombre);
            return ok;
        }

        // Precio y stock son lo que mas interesa controlar: se registra el antes y el despues
        private static string DescribirCambios(Producto antes, Producto despues)
        {
            if (antes == null) return "";
            var cambios = new List<string>();
            if (antes.ProNombre != despues.ProNombre) cambios.Add($"nombre '{antes.ProNombre}' → '{despues.ProNombre}'");
            if (antes.ProPrecio != despues.ProPrecio) cambios.Add($"precio {antes.ProPrecio:C0} → {despues.ProPrecio:C0}");
            if (antes.ProStock != despues.ProStock) cambios.Add($"stock {antes.ProStock} → {despues.ProStock}");
            if (antes.ProDescripcion != despues.ProDescripcion) cambios.Add("descripción");
            return cambios.Count == 0 ? " · sin cambios" : " · " + string.Join(", ", cambios);
        }

        public IEnumerable<Producto> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}