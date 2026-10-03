/**
 * @file ClienteService.cs
 * @brief Reglas de negocio de los clientes.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de los clientes.
     */
    public class ClienteService
    {
        /** Acceso a datos de clientes. */
        private readonly IClienteRepository _repo;
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;
        /** Registro de movimientos. */
        private readonly IAuditoria _auditoria;

        /**
         * @brief Crea el servicio con sus dependencias.
         * @param repo Acceso a datos
         * @param sesion Usuario del turno (permisos)
         * @param auditoria Registro de movimientos
         */
        public ClienteService(IClienteRepository repo, ISesionUsuario sesion, IAuditoria auditoria)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _sesion = sesion ?? throw new ArgumentNullException(nameof(sesion));
            _auditoria = auditoria ?? throw new ArgumentNullException(nameof(auditoria));
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
            ValidarDatos(c);
            decimal id = _repo.Insert(c);
            _auditoria.Registrar("Cliente creado", $"{c.PerNombre} {c.PerApellido}");
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
            ValidarDatos(c);

            bool ok = _repo.Update(c);
            if (ok)
                _auditoria.Registrar("Cliente modificado", $"{c.PerNombre} {c.PerApellido}");
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
            _sesion.ExigirAdmin("eliminar clientes");
            if (id <= 0)
                throw new ArgumentException("Id de cliente inválido");

            int facturas = _repo.CountFacturas(id);
            if (facturas > 0)
                throw new InvalidOperationException($"No se puede eliminar el cliente porque tiene {facturas} factura(s) registrada(s).");

            var cliente = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                _auditoria.Registrar("Cliente eliminado", $"{cliente?.PerNombre} {cliente?.PerApellido}");
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

        /**
         * @brief Reglas comunes al crear y al modificar un cliente.
         * @param c Cliente a validar
         * @exception ArgumentException Si faltan nombre o apellido, o el email no es válido
         */
        private static void ValidarDatos(Cliente c)
        {
            ArgumentNullException.ThrowIfNull(c);
            if (string.IsNullOrWhiteSpace(c.PerNombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(c.PerApellido))
                throw new ArgumentException("El apellido del cliente es obligatorio.");
            if (!string.IsNullOrWhiteSpace(c.PerEmail) && !c.PerEmail.Contains('@'))
                throw new ArgumentException("El email no es válido.");
        }
    }
}
