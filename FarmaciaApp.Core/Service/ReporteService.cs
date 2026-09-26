/**
 * @file ReporteService.cs
 * @brief Reportes de ventas.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Reportes de ventas por periodo para el administrador (fechas incluidas).
     */
    public class ReporteService
    {
        private readonly ReporteRepository _repo = new ReporteRepository();

        /**
         * @brief Totales del periodo.
         * @param desde Fecha inicial
         * @param hasta Fecha final
         * @return Resumen de ventas
         */
        public ResumenVentas ObtenerResumen(DateTime desde, DateTime hasta)
        {
            Validar(desde, hasta);
            return _repo.GetResumen(desde.Date, hasta.Date.AddDays(1));
        }

        /**
         * @brief Ventas agrupadas por vendedor.
         * @param desde Fecha inicial
         * @param hasta Fecha final
         * @return Vendedores ordenados por total
         */
        public IEnumerable<VentaPorVendedor> ObtenerVentasPorVendedor(DateTime desde, DateTime hasta)
        {
            Validar(desde, hasta);
            return _repo.GetVentasPorVendedor(desde.Date, hasta.Date.AddDays(1));
        }

        /**
         * @brief Los 10 productos más vendidos.
         * @param desde Fecha inicial
         * @param hasta Fecha final
         * @return Productos ordenados por unidades
         */
        public IEnumerable<ProductoVendido> ObtenerProductosMasVendidos(DateTime desde, DateTime hasta)
        {
            Validar(desde, hasta);
            return _repo.GetProductosMasVendidos(desde.Date, hasta.Date.AddDays(1));
        }

        /**
         * @brief Verifica el permiso y el rango de fechas.
         * @param desde Fecha inicial
         * @param hasta Fecha final
         * @exception ArgumentException Si la fecha final es anterior a la inicial
         */
        private static void Validar(DateTime desde, DateTime hasta)
        {
            Sesion.ExigirAdmin("ver los reportes de ventas");
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");
        }
    }
}
