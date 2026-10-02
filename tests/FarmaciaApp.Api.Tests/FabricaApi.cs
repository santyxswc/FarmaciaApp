/**
 * @file FabricaApi.cs
 * @brief API en memoria con la base de datos simulada.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using FarmaciaApp.Core.Tests;

namespace FarmaciaApp.Api.Tests;

/**
 * @brief Arranca la API real reemplazando los repositorios de Oracle por dobles.
 *
 * Así se prueban de punta a punta la autenticación, los permisos, la validación y el formato
 * de los errores sin necesitar una base de datos.
 */
public sealed class FabricaApi : WebApplicationFactory<Program>
{
    /** Usuarios guardados, por login. Todos tienen la contraseña "clave123". */
    public Dictionary<string, Usuario> Usuarios { get; } = new()
    {
        ["admin"] = new Usuario { UsuId = 1, Login = "admin", Rol = Usuario.RolAdministrador, Activo = true, Hash = "h:clave123" },
        ["andres"] = new Usuario { UsuId = 2, Login = "andres", Rol = Usuario.RolEmpleado, Activo = true, Hash = "h:clave123", PerId = 5 },
        ["inactivo"] = new Usuario { UsuId = 3, Login = "inactivo", Rol = Usuario.RolEmpleado, Activo = false, Hash = "h:clave123" }
    };

    /** Productos guardados. */
    public List<Producto> Productos { get; } = new()
    {
        new Producto { ProId = 1, ProNombre = "Acetaminofén 500 mg", ProPrecio = 8500, ProStock = 100, ProDescripcion = "Analgésico" },
        new Producto { ProId = 2, ProNombre = "Ibuprofeno 400 mg", ProPrecio = 12000, ProStock = 10 }
    };

    /**
     * @brief Configura la clave JWT y reemplaza la infraestructura.
     * @param builder Constructor del host
     */
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Jwt:Key"] = "clave-de-pruebas-con-mas-de-32-caracteres"
        }));

        builder.ConfigureLogging(registro => registro.ClearProviders());

        builder.ConfigureServices(servicios =>
        {
            servicios.RemoveAll<IHasherClaves>();
            servicios.AddSingleton<IHasherClaves>(new HasherFalso());

            var (usuarios, controlUsuarios) = Stub<IUsuarioRepository>.Crear();
            controlUsuarios
                .Cuando(nameof(IUsuarioRepository.GetByLogin), a => Usuarios.GetValueOrDefault((string)a[0]))
                .Cuando(nameof(IUsuarioRepository.GetById), a => Usuarios.Values.FirstOrDefault(u => u.UsuId == (decimal)a[0]))
                .Cuando(nameof(IUsuarioRepository.RegistrarIngreso), _ => null)
                .Cuando(nameof(IUsuarioRepository.GetVenIdPorPersona), _ => (decimal?)null);
            servicios.RemoveAll<IUsuarioRepository>();
            servicios.AddSingleton(usuarios);

            var (movimientos, controlMovimientos) = Stub<IMovimientoRepository>.Crear();
            controlMovimientos.Cuando(nameof(IMovimientoRepository.Insert), _ => null);
            servicios.RemoveAll<IMovimientoRepository>();
            servicios.AddSingleton(movimientos);

            var (productos, controlProductos) = Stub<IProductoRepository>.Crear();
            controlProductos
                .Cuando(nameof(IProductoRepository.GetAll), _ => Productos.ToList())
                .Cuando(nameof(IProductoRepository.GetById), a => Productos.FirstOrDefault(p => p.ProId == (int)a[0]))
                .Cuando(nameof(IProductoRepository.SearchByName),
                    a => Productos.Where(p => p.ProNombre.Contains((string)a[0], StringComparison.OrdinalIgnoreCase)).ToList())
                .Cuando(nameof(IProductoRepository.Insert), a =>
                {
                    var nuevo = (Producto)a[0];
                    nuevo.ProId = Productos.Max(p => p.ProId) + 1;
                    Productos.Add(nuevo);
                    return nuevo.ProId;
                })
                .Cuando(nameof(IProductoRepository.Update), a =>
                {
                    var cambios = (Producto)a[0];
                    var actual = Productos.First(p => p.ProId == cambios.ProId);
                    actual.ProNombre = cambios.ProNombre;
                    actual.ProPrecio = cambios.ProPrecio;
                    actual.ProStock = cambios.ProStock;
                    return true;
                })
                .Cuando(nameof(IProductoRepository.CountVentas), a => (int)a[0] == 2 ? 3 : 0)
                .Cuando(nameof(IProductoRepository.DeleteCascade), a => Productos.RemoveAll(p => p.ProId == (int)a[0]) > 0);
            servicios.RemoveAll<IProductoRepository>();
            servicios.AddSingleton(productos);
        });
    }
}
