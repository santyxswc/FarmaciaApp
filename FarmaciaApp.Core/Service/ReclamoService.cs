using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services

{
    // CAMBIO CRÍTICO: Debe ser 'public'
    public class ReclamoService
    {
        private readonly ReclamoRepository _repo;

        public ReclamoService()
        {
            // Asumimos que ya implementaste ReclamoRepository
            _repo = new ReclamoRepository();
        }

        // Obtener la lista de Reclamos
        public IEnumerable<Reclamo> ObtenerReclamos() => _repo.GetAll();

        // Obtener un Reclamo por su ID
        public Reclamo ObtenerPorId(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id de reclamo inválido.");

            return _repo.GetById(id);
        }

        // Crear un Reclamo
        public decimal CrearReclamo(Reclamo r)
        {
            // Validaciones
            if (r.FacNumFactura <= 0)
                throw new ArgumentException("El reclamo debe estar asociado a una factura válida.");
            if (string.IsNullOrWhiteSpace(r.RecDescripcion))
                throw new ArgumentException("La descripción del reclamo es obligatoria.");

            // Asumimos que el repositorio tiene un método Insert que devuelve el ID
            return _repo.Insert(r);
        }

        // Eliminar un Reclamo
        public bool EliminarReclamo(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            return _repo.Delete(id);
        }

        // Método para búsqueda de reclamos (por descripción, estado o número de factura)
        public IEnumerable<Reclamo> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerReclamos();
            }
            // Esto es una delegación al repositorio
            return _repo.Search(termino);
        }
    }
}