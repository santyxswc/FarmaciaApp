using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class ReclamoService
    {
        public static readonly string[] Estados = { "Pendiente", "En proceso", "Resuelto", "Rechazado" };

        private readonly ReclamoRepository _repo;

        public ReclamoService()
        {
            _repo = new ReclamoRepository();
        }

        public IEnumerable<Reclamo> ObtenerReclamos() => _repo.GetAll();

        public Reclamo ObtenerPorId(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id de reclamo inválido.");

            return _repo.GetById(id);
        }

        public decimal CrearReclamo(Reclamo r)
        {
            if (r.FacNumFactura <= 0)
                throw new ArgumentException("El reclamo debe estar asociado a una factura válida.");
            if (string.IsNullOrWhiteSpace(r.RecDescripcion))
                throw new ArgumentException("La descripción del reclamo es obligatoria.");

            return _repo.Insert(r);
        }

        public bool ActualizarReclamo(Reclamo r)
        {
            if (r.RecId <= 0)
                throw new ArgumentException("Id de reclamo inválido.");
            if (string.IsNullOrWhiteSpace(r.RecDescripcion))
                throw new ArgumentException("La descripción del reclamo es obligatoria.");

            return _repo.Update(r);
        }

        public bool EliminarReclamo(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            return _repo.Delete(id);
        }

        public IEnumerable<Reclamo> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerReclamos();
            }
            return _repo.Search(termino);
        }
    }
}
