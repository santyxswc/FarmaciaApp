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

            decimal id = _repo.Insert(r);
            Auditoria.Registrar("Reclamo creado", $"N° {id} · factura {r.FacNumFactura}");
            return id;
        }

        public bool ActualizarReclamo(Reclamo r)
        {
            if (r.RecId <= 0)
                throw new ArgumentException("Id de reclamo inválido.");
            if (string.IsNullOrWhiteSpace(r.RecDescripcion))
                throw new ArgumentException("La descripción del reclamo es obligatoria.");

            bool ok = _repo.Update(r);
            if (ok)
                Auditoria.Registrar("Reclamo modificado", $"N° {r.RecId} · estado {r.RecEstado}");
            return ok;
        }

        public bool EliminarReclamo(decimal id)
        {
            Sesion.ExigirAdmin("eliminar reclamos");
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            bool ok = _repo.Delete(id);
            if (ok)
                Auditoria.Registrar("Reclamo eliminado", $"N° {id}");
            return ok;
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
