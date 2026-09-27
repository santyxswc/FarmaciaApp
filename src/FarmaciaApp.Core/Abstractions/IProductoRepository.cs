/**
 * @file IProductoRepository.cs
 * @brief Contrato de acceso a datos de producto.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Operaciones de persistencia de producto que usan los servicios.
     *
     * La implementación con Oracle y Dapper está en FarmaciaApp.Infrastructure.
     */
    public interface IProductoRepository
    {
        /**
         * @brief Obtiene todos los productos ordenados por nombre.
         * @return Productos
         */
        IEnumerable<Producto> GetAll();

        /**
         * @brief Busca un producto.
         * @param id PRO_ID
         * @return Producto, o null si no existe
         */
        Producto GetById(int id);

        /**
         * @brief Registra un producto.
         * @param p Datos del producto
         * @return PRO_ID asignado por SEQ_PRODUCTO
         */
        int Insert(Producto p);

        /**
         * @brief Actualiza un producto.
         * @param p Producto con los datos nuevos
         * @return true si se actualizo
         */
        bool Update(Producto p);

        /**
         * @brief Cuenta las líneas de factura en las que aparece el producto.
         * @param id PRO_ID
         * @return Número de líneas
         */
        int CountVentas(int id);

        /**
         * @brief Elimina el producto y sus relaciones con promociones y proveedores.
         * @param id PRO_ID
         * @return true si se elimino
         */
        bool DeleteCascade(int id);

        /**
         * @brief Busca productos por nombre.
         * @param term Texto a buscar
         * @return Productos que coinciden
         */
        IEnumerable<Producto> SearchByName(string term);
    }
}
