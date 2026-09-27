/**
 * @file Reportes.cs
 * @brief Modelos de los reportes de ventas.
 * @author Santiago Caicedo
 */
namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Totales de ventas de un periodo.
     */
    public class ResumenVentas
    {
        /** Número de facturas. */
        public int Facturas { get; set; }
        /** Total vendido. */
        public decimal Total { get; set; }
        /** Unidades vendidas. */
        public decimal Unidades { get; set; }
        /** Valor promedio por factura. */
        public decimal TicketPromedio => Facturas == 0 ? 0 : System.Math.Round(Total / Facturas, 2);
    }

    /**
     * @brief Ventas de un vendedor en el periodo.
     */
    public class VentaPorVendedor
    {
        /** Nombre del vendedor. */
        public string Vendedor { get; set; }
        /** Facturas que registró. */
        public int Facturas { get; set; }
        /** Total que vendió. */
        public decimal Total { get; set; }
    }

    /**
     * @brief Unidades y total vendidos de un producto en el periodo.
     */
    public class ProductoVendido
    {
        /** Nombre del producto. */
        public string Producto { get; set; }
        /** Unidades vendidas. */
        public decimal Unidades { get; set; }
        /** Total vendido del producto. */
        public decimal Total { get; set; }
    }
}
