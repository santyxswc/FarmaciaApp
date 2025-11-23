using System;
using System.Collections.Generic;
using System.Text;

using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class PromocionService
    {
        private readonly PromocionRepository _repo;

        public PromocionService()
        {
            _repo = new PromocionRepository();
        }

        public IEnumerable<Promocion> ObtenerPromociones() => _repo.GetAll();

        public Promocion ObtenerPorId(int id) => _repo.GetById(id);

        public int CrearPromocion(Promocion p)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(p.PrmDescripcion))
                throw new ArgumentException("La descripción es obligatoria.");
            if (p.PrmDescuento <= 0 || p.PrmDescuento > 100)
                throw new ArgumentException("El descuento debe estar entre 1 y 100%.");
            if (p.PrmFechaIni >= p.PrmFechaFin)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha fin.");

            return _repo.Insert(p);
        }

        public bool ActualizarPromocion(Promocion p)
        {
            if (p.PrmId <= 0)
                throw new ArgumentException("Id de promoción inválido.");
            if (string.IsNullOrWhiteSpace(p.PrmDescripcion))
                throw new ArgumentException("La descripción es obligatoria.");
            if (p.PrmDescuento <= 0 || p.PrmDescuento > 100)
                throw new ArgumentException("El descuento debe estar entre 1 y 100%.");
            if (p.PrmFechaIni >= p.PrmFechaFin)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha fin.");

            return _repo.Update(p);
        }

        public bool EliminarPromocion(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            return _repo.DeleteCascade(id);
        }

        public IEnumerable<Promocion> Buscar(string termino)
        {
            return _repo.SearchByDescription(termino);
        }

        public IEnumerable<Promocion> ObtenerPromocionesActivas()
        {
            return _repo.GetActivas();
        }
    }
}