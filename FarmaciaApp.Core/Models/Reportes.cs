namespace FarmaciaApp.Core.Models
{
    public class ResumenVentas
    {
        public int Facturas { get; set; }
        public decimal Total { get; set; }
        public decimal Unidades { get; set; }
        public decimal TicketPromedio => Facturas == 0 ? 0 : System.Math.Round(Total / Facturas, 2);
    }

    public class VentaPorVendedor
    {
        public string Vendedor { get; set; }
        public int Facturas { get; set; }
        public decimal Total { get; set; }
    }

    public class ProductoVendido
    {
        public string Producto { get; set; }
        public decimal Unidades { get; set; }
        public decimal Total { get; set; }
    }
}
