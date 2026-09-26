using System;
using System.Collections.Generic;
using System.Text;

using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;

namespace FarmaciaApp.Core.Services
{
    public class PersonaService
    {
        private readonly PersonaRepository _repo;

        public PersonaService()
        {
            _repo = new PersonaRepository();
        }

        public IEnumerable<Persona> ObtenerPersonas() => _repo.GetAll();

        public Persona ObtenerPorId(int id) => _repo.GetById(id);

        public int CrearPersona(Persona p)
        {
                        if (string.IsNullOrWhiteSpace(p.PerNombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(p.PerApellido))
                throw new ArgumentException("El apellido es obligatorio.");
            if (!string.IsNullOrWhiteSpace(p.PerEmail) && !p.PerEmail.Contains("@"))
                throw new ArgumentException("El email no es válido.");

            int id = _repo.Insert(p);
            Auditoria.Registrar("Persona creada", p.NombreCompleto);
            return id;
        }

        public bool ActualizarPersona(Persona p)
        {
            if (p.PerId <= 0)
                throw new ArgumentException("Id de persona inválido.");
            if (string.IsNullOrWhiteSpace(p.PerNombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(p.PerApellido))
                throw new ArgumentException("El apellido es obligatorio.");
            if (!string.IsNullOrWhiteSpace(p.PerEmail) && !p.PerEmail.Contains("@"))
                throw new ArgumentException("El email no es válido.");

            bool ok = _repo.Update(p);
            if (ok)
                Auditoria.Registrar("Persona modificada", p.NombreCompleto);
            return ok;
        }

        // Marca o desmarca a la persona como vendedor (quien puede registrar ventas)
        public void AsignarVendedor(int id, bool esVendedor)
        {
            Sesion.ExigirAdmin("cambiar el rol de vendedor");
            if (id <= 0)
                throw new ArgumentException("Id de persona inválido.");

            if (!esVendedor)
            {
                int facturas = _repo.CountFacturasComoVendedor(id);
                if (facturas > 0)
                    throw new InvalidOperationException($"No se puede quitar el rol de vendedor: tiene {facturas} factura(s) registrada(s).");

                if (new UsuarioRepository().CountEmpleadosActivosPorPersona(id) > 0)
                    throw new InvalidOperationException("No se puede quitar el rol de vendedor: tiene una cuenta de empleado activa.");
            }

            _repo.SetVendedor(id, esVendedor);
            Auditoria.Registrar(esVendedor ? "Vendedor asignado" : "Vendedor retirado", _repo.GetById(id)?.NombreCompleto);
        }

        public bool EliminarPersona(int id)
        {
            Sesion.ExigirAdmin("eliminar personas");
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            int facturas = _repo.CountFacturas(id);
            if (facturas > 0)
                throw new InvalidOperationException($"No se puede eliminar: la persona aparece en {facturas} factura(s) como cliente o vendedor.");

            if (new UsuarioRepository().CountPorPersona(id) > 0)
                throw new InvalidOperationException("No se puede eliminar: la persona tiene una cuenta de usuario. Desactiva la cuenta en su lugar.");

            var persona = _repo.GetById(id);
            bool ok = _repo.DeleteCascade(id);
            if (ok)
                Auditoria.Registrar("Persona eliminada", persona?.NombreCompleto);
            return ok;
        }

        public IEnumerable<Persona> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}