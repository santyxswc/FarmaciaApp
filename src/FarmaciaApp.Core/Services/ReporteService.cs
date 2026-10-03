/**
 * @file ReporteService.cs
 * @brief Reportes de ventas.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Reportes de ventas por periodo para el administrador (fechas incluidas).
     */
    public class ReporteService
    {
        /** Acceso a datos de reportes de ventas. */
        private readonly IReporteRepository _repo;
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;

        /**
         * @brief Crea el servicio con sus dependencias.
         * @param repo Acceso a datos
         * @param sesion Usuario del turno (permisos)
         */
        public ReporteService(IReporteRepository repo, ISesionUsuario sesion)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
        }

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
        private void Validar(DateTime desde, DateTime hasta)
        {
            _sesion.ExigirAdmin("ver los reportes de ventas");
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");
        }
    }
}
