/**
 * @file IProveedorRepository.cs
 * @brief Contrato de acceso a datos de proveedor.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Operaciones de persistencia de proveedor que usan los servicios.
     *
     * La implementación con Oracle y Dapper está en FarmaciaApp.Infrastructure.
     */
    public interface IProveedorRepository
    {
        /**
         * @brief Obtiene todos los proveedores ordenados por nombre.
         * @return Proveedores
         */
        IEnumerable<Proveedor> GetAll();

        /**
         * @brief Busca un proveedor.
         * @param id Identificador
         * @return Proveedor, o null si no existe
         */
        Proveedor GetById(int id);

        /**
         * @brief Registra un proveedor.
         * @param p Datos del proveedor
         * @return Identificador asignado
         */
        int Insert(Proveedor p);

        /**
         * @brief Actualiza un proveedor.
         * @param p Proveedor con los datos nuevos
         * @return true si se actualizo
         */
        bool Update(Proveedor p);

        /**
         * @brief Elimina el proveedor y su relación con productos.
         * @param id Identificador
         * @return true si se elimino
         */
        bool DeleteCascade(int id);

        /**
         * @brief Busca proveedores por nombre o contacto.
         * @param term Texto a buscar
         * @return Proveedores que coinciden
         */
        IEnumerable<Proveedor> SearchByName(string term);
    }
}
