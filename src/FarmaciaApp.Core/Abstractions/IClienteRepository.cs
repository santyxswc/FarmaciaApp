/**
 * @file IClienteRepository.cs
 * @brief Contrato de acceso a datos de cliente.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Operaciones de persistencia de cliente que usan los servicios.
     *
     * La implementación con Oracle y Dapper está en FarmaciaApp.Infrastructure.
     */
    public interface IClienteRepository
    {
        /**
         * @brief Obtiene todos los clientes ordenados por apellido.
         * @return Clientes
         */
        IEnumerable<Cliente> GetAll();

        /**
         * @brief Busca un cliente.
         * @param id PER_ID del cliente
         * @return Cliente, o null si no existe
         */
        Cliente GetById(decimal id);

        /**
         * @brief Registra una persona y la marca como cliente en una transacción.
         * @param c Datos del cliente
         * @return PER_ID asignado por SEQ_PERSONA
         */
        decimal Insert(Cliente c);

        /**
         * @brief Actualiza los datos personales de un cliente.
         * @param c Cliente con los datos nuevos
         * @return true si se actualizo
         */
        bool Update(Cliente c);

        /**
         * @brief Cuenta las facturas de un cliente.
         * @param perId PER_ID del cliente
         * @return Número de facturas
         */
        int CountFacturas(decimal perId);

        /**
         * @brief Elimina el cliente y su persona en una transacción.
         * @param id PER_ID del cliente
         * @return true si se elimino
         */
        bool DeleteCascade(decimal id);

        /**
         * @brief Busca clientes por nombre o apellido.
         * @param term Texto a buscar
         * @return Clientes que coinciden
         */
        IEnumerable<Cliente> SearchByName(string term);
    }
}
