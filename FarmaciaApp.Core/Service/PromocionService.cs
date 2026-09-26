/**
 * @file PromocionService.cs
 * @brief Reglas de negocio de las promociones.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de las promociones.
     */
    public class PromocionService
    {
        private readonly PromocionRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public PromocionService()
        {
            _repo = new PromocionRepository();
        }

        /**
         * @brief Obtiene todos los registros.
         * @return Promociones
         */
        public IEnumerable<Promocion> ObtenerPromociones() => _repo.GetAll();

        /**
         * @brief Busca un registro.
         * @param id Identificador
         * @return Registro, o null
         */
        public Promocion ObtenerPorId(int id) => _repo.GetById(id);

        /**
         * @brief Valida y registra. Solo administrador.
         * @param p Datos a registrar
         * @return Identificador asignado
         * @exception ArgumentException Si faltan datos o no son válidos
         */
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

        /**
         * @brief Valida y actualiza. Solo administrador.
         * @param p Registro con los datos nuevos
         * @return true si se actualizo
         * @exception ArgumentException Si faltan datos o no son válidos
         */
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

        /**
         * @brief Elimina el registro y sus relaciones con productos. Solo administrador.
         * @param id Identificador
         * @return true si se elimino
         */
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

        /**
         * @brief Busca promociones por descripción.
         * @param termino Texto a buscar
         * @return Promociones que coinciden
         */
        public IEnumerable<Promocion> Buscar(string termino)
        {
            return _repo.SearchByDescription(termino);
        }

        /**
         * @brief Obtiene las promociones vigentes hoy.
         * @return Promociones activas
         */
        public IEnumerable<Promocion> ObtenerPromocionesActivas()
        {
            return _repo.GetActivas();
        }
    }
}
