/**
 * @file ClienteService.cs
 * @brief Reglas de negocio de los clientes.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de los clientes.
     */
    public class ClienteService
    {
        private readonly ClienteRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public ClienteService()
        {
            _repo = new ClienteRepository();
        }
        /**
         * @brief Obtiene todos los clientes.
         * @return Clientes
         */
        public IEnumerable<Cliente> ObtenerClientes() => _repo.GetAll();
        /**
         * @brief Busca un cliente.
         * @param id PER_ID
         * @return Cliente, o null si no existe
         */
        public Cliente ObtenerPorId(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id de cliente inválido.");

            return _repo.GetById(id);
        }
        /**
         * @brief Valida y registra un cliente.
         * @param c Datos del cliente (nombre y apellido obligatorios)
         * @return PER_ID asignado
         * @exception ArgumentException Si faltan datos o el email no es válido
         */
        public decimal CrearCliente(Cliente c)
        {
            if (string.IsNullOrWhiteSpace(c.PerNombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(c.PerApellido))
                throw new ArgumentException("El apellido del cliente es obligatorio.");
            if (!string.IsNullOrWhiteSpace(c.PerEmail) && !c.PerEmail.Contains("@"))
                throw new ArgumentException("El email no es válido.");
            decimal id = _repo.Insert(c);
            Auditoria.Registrar("Cliente creado", $"{c.PerNombre} {c.PerApellido}");
            return id;
        }
        /**
         * @brief Valida y actualiza un cliente.
         * @param c Cliente con los datos nuevos
         * @return true si se actualizo
         * @exception ArgumentException Si faltan datos o el email no es válido
         */
        public bool ActualizarCliente(Cliente c)
        {
            if (c.PerId <= 0)
                throw new ArgumentException("Id de cliente inválido.");
            if (string.IsNullOrWhiteSpace(c.PerNombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(c.PerApellido))
                throw new ArgumentException("El apellido del cliente es obligatorio.");
            if (!string.IsNullOrWhiteSpace(c.PerEmail) && !c.PerEmail.Contains("@"))
                throw new ArgumentException("El email no es válido.");

            bool ok = _repo.Update(c);
            if (ok)
                Auditoria.Registrar("Cliente modificado", $"{c.PerNombre} {c.PerApellido}");
            return ok;
        }
        /**
         * @brief Elimina un cliente sin facturas. Solo administrador.
         * @param id PER_ID
         * @return true si se elimino
         * @exception InvalidOperationException Si el cliente tiene facturas
         */
        public bool EliminarCliente(decimal id)
        {
            Sesion.ExigirAdmin("eliminar clientes");
            if (id <= 0)
                throw new ArgumentException("Id de cliente inválido");

            int facturas = _repo.CountFacturas(id);
            if (facturas > 0)
                throw new InvalidOperationException($"No se puede eliminar el cliente porque tiene {facturas} factura(s) registrada(s).");

            var cliente = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                Auditoria.Registrar("Cliente eliminado", $"{cliente?.PerNombre} {cliente?.PerApellido}");
            return ok;
        }
        /**
         * @brief Busca clientes por nombre o apellido.
         * @param termino Texto a buscar; vacío devuelve todos
         * @return Clientes que coinciden
         */
        public IEnumerable<Cliente> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerClientes();
            }
            return _repo.SearchByName(termino);
        }
    }
}
