using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Repositories;
using System;
using System.Collections.Generic;
namespace FarmaciaApp.Core.Services
{
    // CAMBIO CRÍTICO: Debe ser 'public'
    public class FacturaService
    {
        private readonly FacturaRepository _repo;

        public FacturaService()
        {
            // Asumimos que ya implementaste FacturaRepository
            _repo = new FacturaRepository();
        }

        // Obtener la lista de Facturas
        public IEnumerable<Factura> ObtenerFacturas() => _repo.GetAll();

        // Obtener una Factura por su número
        public Factura ObtenerPorNumero(decimal numero)
        {
            if (numero <= 0)
                throw new ArgumentException("Número de factura inválido.");

            return _repo.GetById(numero);
        }

        // Método para búsqueda de facturas (por cliente, vendedor o número)
        public IEnumerable<Factura> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerFacturas();
            }
            // Asumimos que el repositorio tiene un método Search (o similar)
            // Esto es una delegación al repositorio (que debe hacer la lógica SQL)
            return _repo.Search(termino);
        }

        // El CRUD de facturas es complejo y a menudo solo se implementa la consulta, 
        // pero puedes agregar los métodos Insert, Update, etc. si los necesitas.
        /*
        public decimal CrearFactura(Factura f) 
        {
            // Lógica de negocio para calcular totales, impuestos, etc.
            // ...
            return _repo.Insert(f);
        }
        */
    }
}