/**
 * @file ProductoService.cs
 * @brief Reglas de negocio de los productos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de los productos.
     */
    public class ProductoService
    {
        private readonly ProductoRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public ProductoService()
        {
            _repo = new ProductoRepository();
        }

        /**
         * @brief Obtiene todos los productos.
         * @return Productos
         */
        public IEnumerable<Producto> ObtenerProductos() => _repo.GetAll();

        /**
         * @brief Busca un producto.
         * @param id PRO_ID
         * @return Producto, o null
         */
        public Producto ObtenerPorId(int id) => _repo.GetById(id);

        /**
         * @brief Valida y registra un producto. Solo administrador.
         * @param p Datos del producto
         * @return PRO_ID asignado
         * @exception ArgumentException Si falta el nombre, el precio no es positivo o el stock es negativo
         */
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
            Auditoria.Registrar("Producto creado", $"{p.ProNombre} · precio {Formato.Moneda(p.ProPrecio)} · stock {p.ProStock}");
            return id;
        }

        /**
         * @brief Valida y actualiza un producto. Solo administrador.
         * @param p Producto con los datos nuevos
         * @return true si se actualizo
         *
         * El movimiento registrado incluye el precio y el stock anteriores y nuevos.
         */
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

        /**
         * @brief Elimina un producto que nunca se ha vendido. Solo administrador.
         * @param id PRO_ID
         * @return true si se elimino
         * @exception InvalidOperationException Si el producto aparece en facturas
         */
        public bool EliminarProducto(int id)
        {
            Sesion.ExigirAdmin("eliminar productos");
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            int ventas = _repo.CountVentas(id);
            if (ventas > 0)
                throw new InvalidOperationException($"No se puede eliminar el producto porque aparece en {ventas} factura(s).");

            var producto = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                Auditoria.Registrar("Producto eliminado", producto?.ProNombre);
            return ok;
        }

        /**
         * @brief Describe lo que cambió entre dos versiones de un producto.
         * @param antes Producto guardado
         * @param despues Producto con los cambios
         * @return Texto como " · precio $ 8.500 → $ 9.000"
         */
        private static string DescribirCambios(Producto antes, Producto despues)
        {
            if (antes == null) return "";
            var cambios = new List<string>();
            if (antes.ProNombre != despues.ProNombre) cambios.Add($"nombre '{antes.ProNombre}' → '{despues.ProNombre}'");
            if (antes.ProPrecio != despues.ProPrecio) cambios.Add($"precio {Formato.Moneda(antes.ProPrecio)} → {Formato.Moneda(despues.ProPrecio)}");
            if (antes.ProStock != despues.ProStock) cambios.Add($"stock {antes.ProStock} → {despues.ProStock}");
            if (antes.ProDescripcion != despues.ProDescripcion) cambios.Add("descripción");
            return cambios.Count == 0 ? " · sin cambios" : " · " + string.Join(", ", cambios);
        }

        /**
         * @brief Busca productos por nombre.
         * @param termino Texto a buscar
         * @return Productos que coinciden
         */
        public IEnumerable<Producto> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}
