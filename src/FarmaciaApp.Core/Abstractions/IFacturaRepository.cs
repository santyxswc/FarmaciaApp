/**
 * @file IFacturaRepository.cs
 * @brief Contrato de acceso a datos de factura.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Operaciones de persistencia de factura que usan los servicios.
     *
     * La implementación con Oracle y Dapper está en FarmaciaApp.Infrastructure.
     */
    public interface IFacturaRepository
    {
        /**
         * @brief Obtiene todas las facturas, de la más reciente a la más antigua.
         * @return Facturas
         */
        IEnumerable<Factura> GetAll();

        /**
         * @brief Busca una factura.
         * @param numero Número de factura
         * @return Factura, o null si no existe
         */
        Factura GetById(decimal numero);

        /**
         * @brief Obtiene las líneas de una factura.
         * @param numero Número de factura
         * @return Lineas con el nombre de cada producto
         */
        List<FacturaProductoDetalle> GetItems(decimal numero);

        /**
         * @brief Busca facturas por cliente, vendedor o número.
         * @param term Texto a buscar
         * @return Facturas que coinciden
         */
        IEnumerable<Factura> Search(string term);

        /**
         * @brief Clientes para la lista de una venta.
         * @return CLI_ID y nombre de cada cliente
         */
        IEnumerable<Seleccion> GetClientes();

        /**
         * @brief Vendedores para la lista de una venta.
         * @return VEN_ID y nombre de cada vendedor
         */
        IEnumerable<Seleccion> GetVendedores();

        /**
         * @brief Productos con el precio del día y su stock.
         * @return Productos con el descuento de la promoción vigente
         */
        IEnumerable<ProductoVenta> GetProductosParaVenta();

        /**
         * @brief Registra una venta completa en una transacción.
         * @param cliId CLI_ID del cliente
         * @param venId VEN_ID del vendedor
         * @param metodoPago Método de pago
         * @param items Productos y cantidades (sin repetidos)
         * @return Número de la factura creada
         * @exception InvalidOperationException Si un producto no existe o no tiene stock suficiente; en ese caso no se guarda nada
         *
         * Guarda el pago, la factura y sus líneas y descuenta el stock. Los precios se toman de la base de datos
         * con la promoción vigente, y cada producto se bloquea con FOR UPDATE hasta el commit para que dos ventas
         * simultáneas no gasten el mismo stock.
         */
        decimal Insert(decimal cliId, decimal venId, string metodoPago, IEnumerable<FacturaProductoDetalle> items);
    }
}
