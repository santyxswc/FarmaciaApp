using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Services
{
    public class ClienteService
    {
        // El servicio usa el Repositorio para la interacción con la base de datos
        private readonly ClienteRepository _repo;

        public ClienteService()
        {
            // Asumimos que tienes (o crearás) la clase ClienteRepository
            _repo = new ClienteRepository();
        }

        // 1. OBTENER TODOS LOS CLIENTES
        // Llama al método del repositorio para obtener la lista completa.
        public IEnumerable<Cliente> ObtenerClientes() => _repo.GetAll();

        // 2. OBTENER POR ID
        public Cliente ObtenerPorId(decimal id) // Usamos decimal para PER_ID (NUMBER en Oracle)
        {
            if (id <= 0)
                throw new ArgumentException("Id de cliente inválido.");

            return _repo.GetById(id);
        }

        // 3. CREAR CLIENTE
        public decimal CrearCliente(Cliente c)
        {
            // **Validaciones de Negocio** (similares a las de Producto)
            if (string.IsNullOrWhiteSpace(c.PerNombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(c.PerApellido))
                throw new ArgumentException("El apellido del cliente es obligatorio.");
            // Agrega más validaciones según necesites (ej. formato de email)

            // El Repositorio debe encargarse de insertar en TBL_PERSONA y TBL_CLIENTE
            // y devolver el ID (que generalmente será el PER_ID).
            return _repo.Insert(c);
        }

        // 4. ACTUALIZAR CLIENTE
        public bool ActualizarCliente(Cliente c)
        {
            if (c.PerId <= 0)
                throw new ArgumentException("Id de cliente inválido.");
            if (string.IsNullOrWhiteSpace(c.PerNombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");
            // Repite validaciones...

            return _repo.Update(c);
        }

        // 5. ELIMINAR CLIENTE
        public bool EliminarCliente(decimal id)
        {
            if (id <= 0)
                throw new ArgumentException("Id de cliente inválido");

            // Delete en cascada desde el repositorio (debe eliminar en TBL_CLIENTE y TBL_PERSONA)
            return _repo.DeleteCascade(id);
        }

        // 6. BUSCAR CLIENTES
        public IEnumerable<Cliente> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerClientes();
            }
            // Asumimos que la búsqueda es por nombre o apellido.
            return _repo.SearchByName(termino);
        }
    }
}