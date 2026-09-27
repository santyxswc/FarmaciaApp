/**
 * @file IProveedorRepository.cs
 * @brief Contrato del repositorio de proveedores.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Acceso a datos de proveedores.
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
