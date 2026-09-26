using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmaciaApp.Core.Services
{
    public class FacturaService
    {
        public static readonly string[] MetodosPago = { "Efectivo", "Tarjeta débito", "Tarjeta crédito", "Transferencia" };

        private readonly FacturaRepository _repo;

        public FacturaService()
        {
            _repo = new FacturaRepository();
        }

        public IEnumerable<Factura> ObtenerFacturas() => _repo.GetAll();

        public Factura ObtenerPorNumero(decimal numero)
        {
            if (numero <= 0)
                throw new ArgumentException("Número de factura inválido.");

            return _repo.GetById(numero);
        }

        // Factura con sus lineas de productos
        public Factura ObtenerDetalle(decimal numero)
        {
            var factura = ObtenerPorNumero(numero);
            if (factura != null)
                factura.Items = _repo.GetItems(numero);
            return factura;
        }

        public IEnumerable<Factura> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerFacturas();
            }
            return _repo.Search(termino);
        }

        public IEnumerable<Seleccion> ObtenerClientes() => _repo.GetClientes();

        public IEnumerable<Seleccion> ObtenerVendedores() => _repo.GetVendedores();

        public IEnumerable<ProductoVenta> ObtenerProductosParaVenta() => _repo.GetProductosParaVenta();

        public decimal CrearFactura(decimal cliId, decimal venId, string metodoPago, IEnumerable<FacturaProductoDetalle> items)
        {
            if (cliId <= 0)
                throw new ArgumentException("Selecciona un cliente.");
            if (venId <= 0)
                throw new ArgumentException("Selecciona un vendedor.");
            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new ArgumentException("Selecciona el método de pago.");

            // Un mismo producto agregado dos veces se une en una sola linea
            var lineas = (items ?? Enumerable.Empty<FacturaProductoDetalle>())
                .GroupBy(i => i.ProId)
                .Select(g => new FacturaProductoDetalle { ProId = g.Key, Cantidad = g.Sum(i => i.Cantidad) })
                .ToList();

            if (lineas.Count == 0)
                throw new ArgumentException("Agrega al menos un producto a la factura.");
            if (lineas.Any(l => l.Cantidad <= 0 || l.Cantidad != Math.Floor(l.Cantidad)))
                throw new ArgumentException("Las cantidades deben ser números enteros mayores a cero.");

            return _repo.Insert(cliId, venId, metodoPago, lineas);
        }
    }
}
