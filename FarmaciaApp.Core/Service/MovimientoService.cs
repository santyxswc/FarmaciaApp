/**
 * @file MovimientoService.cs
 * @brief Consulta del registro de movimientos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Consulta de TBL_MOVIMIENTO para el administrador.
     */
    public class MovimientoService
    {
        private readonly MovimientoRepository _repo = new MovimientoRepository();

        /**
         * @brief Busca movimientos. Solo administrador.
         * @param desde Fecha inicial (incluida)
         * @param hasta Fecha final (incluida)
         * @param usuId Filtrar por usuario, o null
         * @param texto Texto a buscar en la acción o el detalle
         * @return Hasta 500 movimientos
         * @exception ArgumentException Si la fecha final es anterior a la inicial
         */
        public IEnumerable<Movimiento> Buscar(DateTime desde, DateTime hasta, decimal? usuId, string texto)
        {
            Sesion.ExigirAdmin("ver los movimientos");
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");

            return _repo.Buscar(desde.Date, hasta.Date.AddDays(1), usuId, texto);
        }
    }
}
