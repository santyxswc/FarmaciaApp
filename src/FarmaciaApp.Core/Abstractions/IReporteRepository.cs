/**
 * @file IReporteRepository.cs
 * @brief Contrato del repositorio de reportes de ventas.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Acceso a datos de reportes de ventas.
     */
    public interface IReporteRepository
    {
        /**
         * @brief Calcula facturas, total y unidades vendidas.
         * @param desde Fecha inicial
         * @param hasta Fecha final (no incluida)
         * @return Resumen del periodo
         */
        ResumenVentas GetResumen(DateTime desde, DateTime hasta);

        /**
         * @brief Agrupa las ventas por vendedor.
         * @param desde Fecha inicial
         * @param hasta Fecha final (no incluida)
         * @return Vendedores ordenados por total vendido
         */
        IEnumerable<VentaPorVendedor> GetVentasPorVendedor(DateTime desde, DateTime hasta);

        /**
         * @brief Obtiene los 10 productos con más unidades vendidas.
         * @param desde Fecha inicial
         * @param hasta Fecha final (no incluida)
         * @return Productos ordenados por unidades
         */
        IEnumerable<ProductoVendido> GetProductosMasVendidos(DateTime desde, DateTime hasta);
    }
}
