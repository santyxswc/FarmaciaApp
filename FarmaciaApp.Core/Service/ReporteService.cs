using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    // Reportes de ventas por periodo (fechas inclusivas)
    public class ReporteService
    {
        private readonly ReporteRepository _repo = new ReporteRepository();

        public ResumenVentas ObtenerResumen(DateTime desde, DateTime hasta)
        {
            Validar(desde, hasta);
            return _repo.GetResumen(desde.Date, hasta.Date.AddDays(1));
        }

        public IEnumerable<VentaPorVendedor> ObtenerVentasPorVendedor(DateTime desde, DateTime hasta)
        {
            Validar(desde, hasta);
            return _repo.GetVentasPorVendedor(desde.Date, hasta.Date.AddDays(1));
        }

        public IEnumerable<ProductoVendido> ObtenerProductosMasVendidos(DateTime desde, DateTime hasta)
        {
            Validar(desde, hasta);
            return _repo.GetProductosMasVendidos(desde.Date, hasta.Date.AddDays(1));
        }

        private static void Validar(DateTime desde, DateTime hasta)
        {
            Sesion.ExigirAdmin("ver los reportes de ventas");
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");
        }
    }
}
