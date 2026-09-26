namespace FarmaciaApp.Core.Models
{
    // Opcion para listas desplegables (cliente, vendedor, factura...)
    public class Seleccion
    {
        public decimal Id { get; set; }
        public string Nombre { get; set; }

        public override string ToString() => Nombre;
    }
}
