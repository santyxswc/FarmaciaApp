/**
 * @file ProductosEndpoints.cs
 * @brief Endpoints del catálogo de productos.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Api.Endpoints
{
    /**
     * @brief CRUD de productos. Consultar es para cualquier usuario; modificar, solo administrador.
     */
    public static class ProductosEndpoints
    {
        /**
         * @brief Registra las rutas /api/productos.
         * @param rutas Constructor de rutas
         * @return El mismo constructor, para encadenar
         */
        public static IEndpointRouteBuilder MapProductos(this IEndpointRouteBuilder rutas)
        {
            var grupo = rutas.MapGroup("/api/productos").WithTags("Productos").RequireAuthorization();

            grupo.MapGet("/", Listar)
                .WithSummary("Lista los productos; con ?buscar= filtra por nombre")
                .Produces<IEnumerable<ProductoDto>>();

            grupo.MapGet("/{id:int}", Obtener)
                .WithSummary("Obtiene un producto")
                .Produces<ProductoDto>()
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapPost("/", Crear)
                .WithSummary("Crea un producto (administrador)")
                .Produces<ProductoDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden);

            grupo.MapPut("/{id:int}", Actualizar)
                .WithSummary("Modifica un producto (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound);

            grupo.MapDelete("/{id:int}", Eliminar)
                .WithSummary("Elimina un producto que no aparece en facturas (administrador)")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

            return rutas;
        }

        /**
         * @brief Lista o busca productos.
         * @param productos Servicio de productos
         * @param buscar Texto del nombre, opcional
         * @return 200 con los productos
         */
        private static IResult Listar(ProductoService productos, string? buscar)
        {
            var lista = string.IsNullOrWhiteSpace(buscar) ? productos.ObtenerProductos() : productos.Buscar(buscar);
            return Results.Ok(lista.Select(ProductoDto.De));
        }

        /**
         * @brief Busca un producto por su id.
         * @param id PRO_ID
         * @param productos Servicio de productos
         * @return 200 con el producto o 404
         */
        private static IResult Obtener(int id, ProductoService productos)
        {
            var producto = productos.ObtenerPorId(id);
            return producto == null ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Producto no encontrado")
                                    : Results.Ok(ProductoDto.De(producto));
        }

        /**
         * @brief Crea un producto.
         * @param peticion Datos del producto
         * @param productos Servicio de productos
         * @return 201 con el producto creado
         */
        private static IResult Crear(ProductoRequest peticion, ProductoService productos)
        {
            var producto = peticion.ALaEntidad();
            producto.ProId = productos.CrearProducto(producto);
            return Results.Created($"/api/productos/{producto.ProId}", ProductoDto.De(producto));
        }

        /**
         * @brief Modifica un producto.
         * @param id PRO_ID
         * @param peticion Datos nuevos
         * @param productos Servicio de productos
         * @return 204, o 404 si el producto no existe
         */
        private static IResult Actualizar(int id, ProductoRequest peticion, ProductoService productos)
        {
            if (productos.ObtenerPorId(id) == null)
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Producto no encontrado");
            return productos.ActualizarProducto(peticion.ALaEntidad(id)) ? Results.NoContent()
                : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Producto no encontrado");
        }

        /**
         * @brief Elimina un producto.
         * @param id PRO_ID
         * @param productos Servicio de productos
         * @return 204, o 404 si el producto no existe
         */
        private static IResult Eliminar(int id, ProductoService productos)
        {
            if (productos.ObtenerPorId(id) == null)
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Producto no encontrado");
            return productos.EliminarProducto(id) ? Results.NoContent()
                : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Producto no encontrado");
        }
    }
}
