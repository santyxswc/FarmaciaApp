/**
 * @file IPersonaRepository.cs
 * @brief Contrato de acceso a datos de persona.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Operaciones de persistencia de persona que usan los servicios.
     *
     * La implementación con Oracle y Dapper está en FarmaciaApp.Infrastructure.
     */
    public interface IPersonaRepository
    {
        /**
         * @brief Obtiene todas las personas ordenadas por nombre.
         * @return Personas con sus roles
         */
        IEnumerable<Persona> GetAll();

        /**
         * @brief Busca una persona.
         * @param id PER_ID
         * @return Persona, o null si no existe
         */
        Persona GetById(int id);

        /**
         * @brief Registra una persona.
         * @param p Datos de la persona
         * @return PER_ID asignado por SEQ_PERSONA (la misma secuencia que usan los clientes)
         */
        int Insert(Persona p);

        /**
         * @brief Actualiza los datos personales.
         * @param p Persona con los datos nuevos
         * @return true si se actualizo
         */
        bool Update(Persona p);

        /**
         * @brief Cuenta las facturas donde la persona es cliente o vendedor.
         * @param id PER_ID
         * @return Número de facturas
         */
        int CountFacturas(int id);

        /**
         * @brief Cuenta las facturas que la persona registró como vendedor.
         * @param id PER_ID
         * @return Número de facturas
         */
        int CountFacturasComoVendedor(int id);

        /**
         * @brief Agrega o quita a la persona de TBL_VENDEDOR.
         * @param id PER_ID
         * @param esVendedor true para agregarla, false para quitarla
         */
        void SetVendedor(int id, bool esVendedor);

        /**
         * @brief Elimina la persona y sus registros de cliente y vendedor en una transacción.
         * @param id PER_ID
         * @return true si se elimino
         */
        bool DeleteCascade(int id);

        /**
         * @brief Busca personas por nombre, apellido o email.
         * @param term Texto a buscar
         * @return Personas que coinciden
         */
        IEnumerable<Persona> SearchByName(string term);
    }
}
