/**
 * @file IReclamoRepository.cs
 * @brief Contrato del repositorio de reclamos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Acceso a datos de reclamos.
     */
    public interface IReclamoRepository
    {
        /**
         * @brief Obtiene todos los reclamos, del más reciente al más antiguo.
         * @return Reclamos
         */
        IEnumerable<Reclamo> GetAll();

        /**
         * @brief Busca un reclamo.
         * @param id Número del reclamo
         * @return Reclamo, o null si no existe
         */
        Reclamo GetById(decimal id);

        /**
         * @brief Registra un reclamo con la fecha actual.
         * @param r Datos del reclamo (sin estado queda Pendiente)
         * @return Número asignado por SEQ_RECLAMO
         */
        decimal Insert(Reclamo r);

        /**
         * @brief Actualiza la descripción y el estado de un reclamo.
         * @param r Reclamo con los datos nuevos
         * @return true si se actualizo
         */
        bool Update(Reclamo r);

        /**
         * @brief Elimina un reclamo y sus reintegros en una transacción.
         * @param id Número del reclamo
         * @return true si se elimino
         */
        bool Delete(decimal id);

        /**
         * @brief Busca reclamos por descripción, estado, cliente o número de factura.
         * @param term Texto a buscar
         * @return Reclamos que coinciden
         */
        IEnumerable<Reclamo> Search(string term);

        /**
         * @brief Obtiene los reclamos con un estado.
         * @param estado Estado buscado
         * @return Reclamos en ese estado
         */
        IEnumerable<Reclamo> GetByEstado(string estado);

        /**
         * @brief Obtiene los reclamos de una factura.
         * @param facNumFactura Número de factura
         * @return Reclamos de la factura
         */
        IEnumerable<Reclamo> GetByFactura(decimal facNumFactura);
    }
}
