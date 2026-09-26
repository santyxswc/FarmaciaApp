using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class ClienteService
    {
        private readonly ClienteRepository _repo;

        public ClienteService()
        {
            _repo = new ClienteRepository();
        }
        public IEnumerable<Cliente> ObtenerClientes() => _repo.GetAll();
        public Cliente ObtenerPorId(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id de cliente inválido.");

            return _repo.GetById(id);
        }
        public decimal CrearCliente(Cliente c)
        {
            if (string.IsNullOrWhiteSpace(c.PerNombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(c.PerApellido))
                throw new ArgumentException("El apellido del cliente es obligatorio.");
            if (!string.IsNullOrWhiteSpace(c.PerEmail) && !c.PerEmail.Contains("@"))
                throw new ArgumentException("El email no es válido.");
            return _repo.Insert(c);
        }
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

            return _repo.Update(c);
        }
        public bool EliminarCliente(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id de cliente inválido");

            int facturas = _repo.CountFacturas(id);
            if (facturas > 0)
                throw new InvalidOperationException($"No se puede eliminar el cliente porque tiene {facturas} factura(s) registrada(s).");

            return _repo.DeleteCascade(id);
        }
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