/**
 * @file Dtos.cs
 * @brief Contratos de entrada y salida de la API.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Api.Endpoints
{
    /** Credenciales para iniciar sesión. */
    public sealed record LoginRequest(string Login, string Clave);

    /** Usuario autenticado, sin hash ni sal. */
    public sealed record UsuarioDto(decimal Id, string Login, string Nombre, string Rol)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param u Usuario
         * @return Datos públicos del usuario
         */
        public static UsuarioDto De(Usuario u) => new(u.UsuId, u.Login, u.Nombre, u.Rol);
    }

    /** Respuesta del inicio de sesión. */
    public sealed record LoginResponse(string Token, DateTime Expira, UsuarioDto Usuario);

    /** Producto del catálogo. */
    public sealed record ProductoDto(int Id, string Nombre, decimal Precio, int Stock, string Descripcion, bool StockBajo)
    {
        /**
         * @brief Convierte el modelo de Core.
         * @param p Producto
         * @return Producto para la API
         */
        public static ProductoDto De(Producto p) => new(p.ProId, p.ProNombre, p.ProPrecio, p.ProStock, p.ProDescripcion, p.StockBajo);
    }

    /** Datos para crear o modificar un producto. */
    public sealed record ProductoRequest(string Nombre, decimal Precio, int Stock, string Descripcion)
    {
        /**
         * @brief Convierte la petición al modelo de Core.
         * @param id PRO_ID; 0 al crear
         * @return Producto listo para el servicio
         */
        public Producto ALaEntidad(int id = 0) => new()
        {
            ProId = id,
            ProNombre = Nombre,
            ProPrecio = Precio,
            ProStock = Stock,
            ProDescripcion = Descripcion
        };
    }
}
