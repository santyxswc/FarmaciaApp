/**
 * @file FacturasEndpoints.cs
 * @brief Endpoints de ventas y facturas.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /** Línea de una factura. */
    public sealed record LineaFacturaDto(decimal ProductoId, string Producto, decimal Cantidad, decimal PrecioUnitario, decimal Subtotal);

    /** Factura; Items solo viene en el detalle. */
    public sealed record FacturaDto(
        decimal Numero, DateTime Fecha, decimal Subtotal, decimal Iva, decimal Total,
        decimal ClienteId, decimal VendedorId, string Cliente, string Vendedor, string MetodoPago,
        IReadOnlyList<LineaFacturaDto> Items)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param f Factura, con o sin líneas
         * @return Factura para la API
         */
        public static FacturaDto De(Factura f) => new(
            f.FacNumFactura, f.FacFecha, f.FacSubtotal, f.FacIva, f.FacTotal,
            f.CliId, f.VenId, f.ClienteNombre, f.VendedorNombre, f.MetodoPago,
            f.Items?.Select(i => new LineaFacturaDto(i.ProId, i.ProNombre, i.Cantidad, i.PrecioUnitario, i.SubtotalLinea)).ToList());
    }

    /** Producto y cantidad de una venta. */
    public sealed record LineaVentaRequest(decimal ProductoId, decimal Cantidad);

    /**
     * Venta a registrar. Los precios los toma la base de datos; un empleado vende siempre a su nombre,
     * así que VendedorId solo cuenta para el administrador.
     */
    public sealed record VentaRequest(decimal ClienteId, decimal? VendedorId, string MetodoPago, List<LineaVentaRequest> Items);

    /** Producto con el precio del día. */
    public sealed record ProductoVentaDto(int Id, string Nombre, decimal PrecioBase, decimal Descuento, decimal PrecioFinal, int Stock);

    /** Opción de una lista desplegable. */
    public sealed record OpcionDto(decimal Id, string Nombre);

    /**
     * @brief Consulta de facturas y registro de ventas.
     */
    public static class FacturasEndpoints
    {
        /**
         * @brief Registra las rutas /api/facturas y /api/ventas.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapFacturas(this IEndpointRouteBuilder rutas)
        {
            var facturas = rutas.MapGroup("/api/facturas").WithTags("Facturas").RequireAuthorization();

            facturas.MapGet("/", (FacturaService servicio, string buscar) =>
                    Results.Ok(servicio.Buscar(buscar).Select(FacturaDto.De)))
                .WithSummary("Lista las facturas; con ?buscar= filtra por cliente, vendedor o número")
                .Produces<IEnumerable<FacturaDto>>();

            facturas.MapGet("/{numero:decimal}", (decimal numero, FacturaService servicio) =>
                    servicio.ObtenerDetalle(numero) is { } f ? Results.Ok(FacturaDto.De(f)) : Respuestas.NoEncontrado("Factura"))
                .WithSummary("Factura con sus líneas")
                .Produces<FacturaDto>()
                .ProducesProblem(StatusCodes.Status404NotFound);

            var ventas = rutas.MapGroup("/api/ventas").WithTags("Ventas").RequireAuthorization();

            ventas.MapPost("/", RegistrarVenta)
                .WithSummary("Registra una venta completa o no la registra: valida stock y descuenta en una transacción")
                .Produces<FacturaDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict);

            ventas.MapGet("/clientes", (FacturaService servicio) =>
                    Results.Ok(servicio.ObtenerClientes().Select(s => new OpcionDto(s.Id, s.Nombre))))
                .WithSummary("Clientes que se pueden facturar (CLI_ID)")
                .Produces<IEnumerable<OpcionDto>>();

            ventas.MapGet("/vendedores", (FacturaService servicio) =>
                    Results.Ok(servicio.ObtenerVendedores().Select(s => new OpcionDto(s.Id, s.Nombre))))
                .WithSummary("Vendedores registrados (VEN_ID)")
                .Produces<IEnumerable<OpcionDto>>();

            ventas.MapGet("/productos", (FacturaService servicio) =>
                    Results.Ok(servicio.ObtenerProductosParaVenta()
                        .Select(p => new ProductoVentaDto(p.ProId, p.ProNombre, p.PrecioBase, p.Descuento, p.PrecioFinal, p.Stock))))
                .WithSummary("Productos con el precio del día y el stock")
                .Produces<IEnumerable<ProductoVentaDto>>();

            return rutas;
        }

        /**
         * @brief Valida y registra una venta.
         * @param peticion Cliente, vendedor, método de pago y líneas
         * @param servicio Servicio de facturas
         * @return 201 con la factura creada y sus líneas
         */
        private static IResult RegistrarVenta(VentaRequest peticion, FacturaService servicio)
        {
            var lineas = (peticion.Items ?? new()).Select(i => new FacturaProductoDetalle { ProId = i.ProductoId, Cantidad = i.Cantidad });
            decimal numero = servicio.CrearFactura(peticion.ClienteId, peticion.VendedorId ?? 0, peticion.MetodoPago, lineas);
            return Results.Created($"/api/facturas/{numero}", FacturaDto.De(servicio.ObtenerDetalle(numero)));
        }
    }
}
