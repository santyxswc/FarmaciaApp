/**
 * @file FacturaService.cs
 * @brief Reglas de negocio de las ventas y facturas.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Consulta de facturas y registro de ventas.
     */
    public class FacturaService
    {
        /** Metodos de pago aceptados. */
        public static readonly string[] MetodosPago = { "Efectivo", "Tarjeta débito", "Tarjeta crédito", "Transferencia" };

        private readonly FacturaRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public FacturaService()
        {
            _repo = new FacturaRepository();
        }

        /**
         * @brief Obtiene todas las facturas.
         * @return Facturas
         */
        public IEnumerable<Factura> ObtenerFacturas() => _repo.GetAll();

        /**
         * @brief Busca una factura sin sus líneas.
         * @param numero Número de factura
         * @return Factura, o null
         */
        public Factura ObtenerPorNumero(decimal numero)
        {
            if (numero <= 0)
                throw new ArgumentException("Número de factura inválido.");

            return _repo.GetById(numero);
        }

        /**
         * @brief Busca una factura con sus líneas de productos.
         * @param numero Número de factura
         * @return Factura con Items cargado, o null
         */
        public Factura ObtenerDetalle(decimal numero)
        {
            var factura = ObtenerPorNumero(numero);
            if (factura != null)
                factura.Items = _repo.GetItems(numero);
            return factura;
        }

        /**
         * @brief Busca facturas por cliente, vendedor o número.
         * @param termino Texto a buscar; vacío devuelve todas
         * @return Facturas que coinciden
         */
        public IEnumerable<Factura> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerFacturas();
            }
            return _repo.Search(termino);
        }

        /**
         * @brief Clientes que se pueden facturar.
         * @return CLI_ID y nombre
         */
        public IEnumerable<Seleccion> ObtenerClientes() => _repo.GetClientes();

        /**
         * @brief Vendedores registrados.
         * @return VEN_ID y nombre
         */
        public IEnumerable<Seleccion> ObtenerVendedores() => _repo.GetVendedores();

        /**
         * @brief Productos con el precio del día.
         * @return Productos con descuento y stock
         */
        public IEnumerable<ProductoVenta> ObtenerProductosParaVenta() => _repo.GetProductosParaVenta();

        /**
         * @brief Valida y registra una venta.
         * @param cliId CLI_ID del cliente
         * @param venId VEN_ID del vendedor; para un empleado se usa siempre el suyo
         * @param metodoPago Método de pago
         * @param items Productos y cantidades; los repetidos se unen en una línea
         * @return Número de la factura creada
         * @exception ArgumentException Si faltan datos o hay cantidades no válidas
         * @exception InvalidOperationException Si no hay stock suficiente o el empleado no es vendedor
         */
        public decimal CrearFactura(decimal cliId, decimal venId, string metodoPago, IEnumerable<FacturaProductoDetalle> items)
        {
            Sesion.ExigirSesion();
            if (!Sesion.EsAdmin)
                venId = Sesion.VenId ?? throw new InvalidOperationException(
                    "Tu usuario no está asociado a un vendedor. Pide al administrador que lo configure.");

            if (cliId <= 0)
                throw new ArgumentException("Selecciona un cliente.");
            if (venId <= 0)
                throw new ArgumentException("Selecciona un vendedor.");
            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new ArgumentException("Selecciona el método de pago.");

            var lineas = (items ?? Enumerable.Empty<FacturaProductoDetalle>())
                .GroupBy(i => i.ProId)
                .Select(g => new FacturaProductoDetalle { ProId = g.Key, Cantidad = g.Sum(i => i.Cantidad) })
                .ToList();

            if (lineas.Count == 0)
                throw new ArgumentException("Agrega al menos un producto a la factura.");
            if (lineas.Any(l => l.Cantidad <= 0 || l.Cantidad != Math.Floor(l.Cantidad)))
                throw new ArgumentException("Las cantidades deben ser números enteros mayores a cero.");

            decimal numero = _repo.Insert(cliId, venId, metodoPago, lineas);
            var factura = _repo.GetById(numero);
            Auditoria.Registrar("Venta registrada",
                $"Factura N° {numero} · {factura?.ClienteNombre} · {Formato.Moneda(factura?.FacTotal ?? 0)} · {metodoPago} · vendedor {factura?.VendedorNombre}");
            return numero;
        }
    }
}
