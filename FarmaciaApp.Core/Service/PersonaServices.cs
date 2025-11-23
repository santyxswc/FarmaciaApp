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
            // Validaciones
            if (string.IsNullOrWhiteSpace(p.PerNombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(p.PerApellido))
                throw new ArgumentException("El apellido es obligatorio.");
            if (!string.IsNullOrWhiteSpace(p.PerEmail) && !p.PerEmail.Contains("@"))
                throw new ArgumentException("El email no es válido.");

            return _repo.Insert(p);
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

            return _repo.Update(p);
        }

        public bool EliminarPersona(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            return _repo.DeleteCascade(id);
        }

        public IEnumerable<Persona> Buscar(string termino)
        {
            return _repo.SearchByName(termino);
        }
    }
}