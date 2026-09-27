/**
 * @file IPromocionRepository.cs
 * @brief Contrato de acceso a datos de promocion.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Operaciones de persistencia de promocion que usan los servicios.
     *
     * La implementación con Oracle y Dapper está en FarmaciaApp.Infrastructure.
     */
    public interface IPromocionRepository
    {
        /**
         * @brief Obtiene todas las promociones, de la más reciente a la más antigua.
         * @return Promociones
         */
        IEnumerable<Promocion> GetAll();

        /**
         * @brief Busca una promoción.
         * @param id PRM_ID
         * @return Promoción, o null si no existe
         */
        Promocion GetById(int id);

        /**
         * @brief Registra una promoción.
         * @param p Datos de la promoción
         * @return PRM_ID asignado
         */
        int Insert(Promocion p);

        /**
         * @brief Actualiza una promoción.
         * @param p Promoción con los datos nuevos
         * @return true si se actualizo
         */
        bool Update(Promocion p);

        /**
         * @brief Elimina la promoción y su relación con productos.
         * @param id PRM_ID
         * @return true si se elimino
         */
        bool DeleteCascade(int id);

        /**
         * @brief Busca promociones por descripción.
         * @param term Texto a buscar
         * @return Promociones que coinciden
         */
        IEnumerable<Promocion> SearchByDescription(string term);

        /**
         * @brief Obtiene las promociones vigentes hoy.
         * @return Promociones activas
         */
        IEnumerable<Promocion> GetActivas();
    }
}
