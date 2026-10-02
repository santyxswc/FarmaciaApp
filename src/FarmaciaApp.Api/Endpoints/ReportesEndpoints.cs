/**
 * @file ReportesEndpoints.cs
 * @brief Endpoints de reportes de ventas.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Totales de un periodo. */
    public sealed record ResumenVentasDto(int Facturas, decimal Total, decimal Unidades, decimal TicketPromedio);

    /** Ventas de un empleado en un periodo. */
    public sealed record VentaPorVendedorDto(string Vendedor, int Facturas, decimal Total);

    /** Producto con sus ventas en un periodo. */
    public sealed record ProductoVendidoDto(string Producto, decimal Unidades, decimal Total);

    /**
     * @brief Reportes de ventas. Solo administrador; el periodo por defecto son los últimos 7 días.
     */
    public static class ReportesEndpoints
    {
        /**
         * @brief Registra las rutas /api/reportes.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapReportes(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/reportes").WithTags("Reportes").RequireAuthorization();

            grupo.MapGet("/resumen", (ReporteService reportes, DateOnly? desde, DateOnly? hasta) =>
                {
                    var (ini, fin) = Respuestas.Periodo(desde, hasta);
                    var r = reportes.ObtenerResumen(ini, fin);
                    return Results.Ok(new ResumenVentasDto(r.Facturas, r.Total, r.Unidades, r.TicketPromedio));
                })
                .WithSummary("Total vendido, facturas, unidades y ticket promedio (administrador)")
                .Produces<ResumenVentasDto>()
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapGet("/vendedores", (ReporteService reportes, DateOnly? desde, DateOnly? hasta) =>
                {
                    var (ini, fin) = Respuestas.Periodo(desde, hasta);
                    return Results.Ok(reportes.ObtenerVentasPorVendedor(ini, fin).Select(v => new VentaPorVendedorDto(v.Vendedor, v.Facturas, v.Total)));
                })
                .WithSummary("Ventas por empleado (administrador)")
                .Produces<IEnumerable<VentaPorVendedorDto>>()
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapGet("/productos-mas-vendidos", (ReporteService reportes, DateOnly? desde, DateOnly? hasta) =>
                {
                    var (ini, fin) = Respuestas.Periodo(desde, hasta);
                    return Results.Ok(reportes.ObtenerProductosMasVendidos(ini, fin).Select(p => new ProductoVendidoDto(p.Producto, p.Unidades, p.Total)));
                })
                .WithSummary("Los 10 productos más vendidos (administrador)")
                .Produces<IEnumerable<ProductoVendidoDto>>()
                .ProducesProblem(StatusCodes.Status403Forbidden);

            return rutas;
        }
    }
}
