using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class MovimientoService
    {
        private readonly MovimientoRepository _repo = new MovimientoRepository();

        public IEnumerable<Movimiento> Buscar(DateTime desde, DateTime hasta, decimal? usuId, string texto)
        {
            Sesion.ExigirAdmin("ver los movimientos");
            if (hasta.Date < desde.Date)
                throw new ArgumentException("La fecha final no puede ser anterior a la inicial.");

            return _repo.Buscar(desde.Date, hasta.Date.AddDays(1), usuId, texto);
        }
    }
}
