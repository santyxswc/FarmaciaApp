/**
 * @file MovimientoService.cs
 * @brief Consulta del registro de movimientos.
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
     * @brief Consulta de TBL_MOVIMIENTO para el administrador.
     */
    public class MovimientoService
    {
        /** Acceso a datos de movimientos. */
        private readonly IMovimientoRepository _repo;
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;

        /**
         * @brief Crea el servicio con sus dependencias.
         * @param repo Acceso a datos
         * @param sesion Usuario del turno (permisos)
         */
        public MovimientoService(IMovimientoRepository repo, ISesionUsuario sesion)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
        }

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
            _sesion.ExigirAdmin("ver los movimientos");
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");

            return _repo.Buscar(desde.Date, hasta.Date.AddDays(1), usuId, texto);
        }
    }
}
