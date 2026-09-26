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
            Sesion.ExigirAdmin("crear promociones");
                        if (string.IsNullOrWhiteSpace(p.PrmDescripcion))
                throw new ArgumentException("La descripción es obligatoria.");
            if (p.PrmDescuento <= 0 || p.PrmDescuento > 100)
                throw new ArgumentException("El descuento debe estar entre 1 y 100%.");
            if (p.PrmFechaIni >= p.PrmFechaFin)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha fin.");

            int id = _repo.Insert(p);
            Auditoria.Registrar("Promoción creada", p.PrmDescripcion);
            return id;
        }

        public bool ActualizarPromocion(Promocion p)
        {
            Sesion.ExigirAdmin("modificar promociones");
            if (p.PrmId <= 0)
                throw new ArgumentException("Id de promoción inválido.");
            if (string.IsNullOrWhiteSpace(p.PrmDescripcion))
                throw new ArgumentException("La descripción es obligatoria.");
            if (p.PrmDescuento <= 0 || p.PrmDescuento > 100)
                throw new ArgumentException("El descuento debe estar entre 1 y 100%.");
            if (p.PrmFechaIni >= p.PrmFechaFin)
                throw new ArgumentException("La fecha de inicio debe ser anterior a la fecha fin.");

            bool ok = _repo.Update(p);
            if (ok)
                Auditoria.Registrar("Promoción modificada", p.PrmDescripcion);
            return ok;
        }

        public bool EliminarPromocion(int id)
        {
            Sesion.ExigirAdmin("eliminar promociones");
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            var registro = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                Auditoria.Registrar("Promoción eliminada", registro?.PrmDescripcion);
            return ok;
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