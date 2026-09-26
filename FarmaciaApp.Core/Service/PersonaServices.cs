/**
 * @file PersonaServices.cs
 * @brief Reglas de negocio de las personas.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;

using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Validaciones, permisos y registro de movimientos de las personas.
     */
    public class PersonaService
    {
        private readonly PersonaRepository _repo;

        /**
         * @brief Crea el servicio con su repositorio.
         */
        public PersonaService()
        {
            _repo = new PersonaRepository();
        }

        /**
         * @brief Obtiene todas las personas.
         * @return Personas con sus roles
         */
        public IEnumerable<Persona> ObtenerPersonas() => _repo.GetAll();

        /**
         * @brief Busca una persona.
         * @param id PER_ID
         * @return Persona, o null
         */
        public Persona ObtenerPorId(int id) => _repo.GetById(id);

        /**
         * @brief Valida y registra una persona.
         * @param p Datos de la persona (nombre y apellido obligatorios)
         * @return PER_ID asignado
         * @exception ArgumentException Si faltan datos o el email no es válido
         */
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

        /**
         * @brief Valida y actualiza los datos personales. No cambia los roles.
         * @param p Persona con los datos nuevos
         * @return true si se actualizo
         * @exception ArgumentException Si faltan datos o el email no es válido
         */
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

        /**
         * @brief Marca o desmarca a una persona como vendedor. Solo administrador.
         * @param id PER_ID
         * @param esVendedor true para marcarla como vendedor
         * @exception InvalidOperationException Si se quita el rol a alguien con facturas o con cuenta de empleado activa
         */
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

        /**
         * @brief Elimina una persona. Solo administrador.
         * @param id PER_ID
         * @return true si se elimino
         * @exception InvalidOperationException Si aparece en facturas o tiene una cuenta de usuario
         */
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

        /**
         * @brief Busca personas por nombre, apellido o email.
         * @param termino Texto a buscar
         * @return Personas que coinciden
         */
        public IEnumerable<Persona> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}
