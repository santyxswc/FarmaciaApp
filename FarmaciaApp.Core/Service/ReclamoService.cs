/**
 * @file ReclamoService.cs
 * @brief Reglas de negocio de los reclamos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de los reclamos.
     */
    public class ReclamoService
    {
        /** Estados posibles de un reclamo. */
        public static readonly string[] Estados = { "Pendiente", "En proceso", "Resuelto", "Rechazado" };

        private readonly ReclamoRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public ReclamoService()
        {
            _repo = new ReclamoRepository();
        }

        /**
         * @brief Obtiene todos los reclamos.
         * @return Reclamos
         */
        public IEnumerable<Reclamo> ObtenerReclamos() => _repo.GetAll();

        /**
         * @brief Busca un reclamo.
         * @param id Número del reclamo
         * @return Reclamo, o null
         */
        public Reclamo ObtenerPorId(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id de reclamo inválido.");

            return _repo.GetById(id);
        }

        /**
         * @brief Valida y registra un reclamo.
         * @param r Reclamo con factura y descripción
         * @return Número asignado
         * @exception ArgumentException Si falta la factura o la descripción
         */
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

        /**
         * @brief Valida y actualiza la descripción y el estado.
         * @param r Reclamo con los datos nuevos
         * @return true si se actualizo
         * @exception ArgumentException Si falta la descripción
         */
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

        /**
         * @brief Elimina un reclamo. Solo administrador.
         * @param id Número del reclamo
         * @return true si se elimino
         */
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

        /**
         * @brief Busca reclamos.
         * @param termino Texto a buscar; vacío devuelve todos
         * @return Reclamos que coinciden
         */
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
