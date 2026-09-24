using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class FacturaService
    {
        private readonly FacturaRepository _repo;

        public FacturaService()
        {
            _repo = new FacturaRepository();
        }

        public IEnumerable<Factura> ObtenerFacturas() => _repo.GetAll();

        public Factura ObtenerPorNumero(decimal numero)
        {
            if (numero <= 0)
                throw new ArgumentException("Número de factura inválido.");

            return _repo.GetById(numero);
        }

        public IEnumerable<Factura> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerFacturas();
            }
            return _repo.Search(termino);
        }
    }
}
