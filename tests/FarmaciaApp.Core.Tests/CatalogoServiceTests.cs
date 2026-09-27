/**
 * @file CatalogoServiceTests.cs
 * @brief Pruebas de los servicios de productos, clientes y facturas.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Core.Tests;

/**
 * @brief Pruebas de ProductoService.
 */
public class ProductoServiceTests
{
    /** Repositorio de productos simulado. */
    private readonly (IProductoRepository Objeto, Stub<IProductoRepository> Control) repo = Stub<IProductoRepository>.Crear();
    /** Movimientos registrados. */
    private readonly AuditoriaEnMemoria auditoria = new();

    /**
     * @brief Un empleado no puede crear productos.
     */
    [Fact]
    public void Un_empleado_no_puede_crear_productos() =>
        Assert.Throws<UnauthorizedAccessException>(() =>
            new ProductoService(repo.Objeto, Sesiones.Empleado(), auditoria).CrearProducto(new Producto { ProNombre = "X", ProPrecio = 1 }));

    /**
     * @brief Crear y actualizar rechazan los mismos datos no válidos.
     * @param nombre Nombre
     * @param precio Precio
     * @param stock Stock
     */
    [Theory]
    [InlineData("", 1000, 5)]
    [InlineData("Ibuprofeno", 0, 5)]
    [InlineData("Ibuprofeno", 1000, -1)]
    public void Crear_y_actualizar_comparten_la_validacion(string nombre, int precio, int stock)
    {
        var servicio = new ProductoService(repo.Objeto, Sesiones.Admin(), auditoria);
        var producto = new Producto { ProId = 1, ProNombre = nombre, ProPrecio = precio, ProStock = stock };
        Assert.Throws<ArgumentException>(() => servicio.CrearProducto(producto));
        Assert.Throws<ArgumentException>(() => servicio.ActualizarProducto(producto));
    }

    /**
     * @brief No se elimina un producto que aparece en facturas.
     */
    [Fact]
    public void No_se_elimina_un_producto_que_ya_se_vendio()
    {
        repo.Control.Cuando(nameof(IProductoRepository.CountVentas), _ => 3);
        var error = Assert.Throws<InvalidOperationException>(() =>
            new ProductoService(repo.Objeto, Sesiones.Admin(), auditoria).EliminarProducto(8));
        Assert.Contains("3 factura", error.Message);
    }

    /**
     * @brief Un producto válido se guarda y se registra el movimiento.
     */
    [Fact]
    public void Crear_un_producto_valido_lo_guarda_y_lo_audita()
    {
        repo.Control.Cuando(nameof(IProductoRepository.Insert), _ => 42);
        int id = new ProductoService(repo.Objeto, Sesiones.Admin(), auditoria)
            .CrearProducto(new Producto { ProNombre = "Ibuprofeno", ProPrecio = 1000, ProStock = 50 });
        Assert.Equal(42, id);
        Assert.Contains("Producto creado", auditoria.Acciones);
    }
}

/**
 * @brief Pruebas de ClienteService.
 */
public class ClienteServiceTests
{
    /** Repositorio de clientes simulado. */
    private readonly (IClienteRepository Objeto, Stub<IClienteRepository> Control) repo = Stub<IClienteRepository>.Crear();

    /**
     * @brief Rechaza clientes sin nombre, sin apellido o con email no válido.
     * @param nombre Nombre
     * @param apellido Apellido
     * @param email Email
     */
    [Theory]
    [InlineData(null, "Pérez", null)]
    [InlineData("Ana", " ", null)]
    [InlineData("Ana", "Pérez", "correo-invalido")]
    public void Valida_nombre_apellido_y_email(string nombre, string apellido, string email) =>
        Assert.Throws<ArgumentException>(() =>
            new ClienteService(repo.Objeto, Sesiones.Empleado(), new AuditoriaEnMemoria())
                .CrearCliente(new Cliente { PerNombre = nombre, PerApellido = apellido, PerEmail = email }));

    /**
     * @brief No se elimina un cliente con facturas.
     */
    [Fact]
    public void No_se_elimina_un_cliente_con_facturas()
    {
        repo.Control.Cuando(nameof(IClienteRepository.CountFacturas), _ => 2);
        Assert.Throws<InvalidOperationException>(() =>
            new ClienteService(repo.Objeto, Sesiones.Admin(), new AuditoriaEnMemoria()).EliminarCliente(4));
        Assert.Equal(0, repo.Control.Veces(nameof(IClienteRepository.DeleteCascade)));
    }
}

/**
 * @brief Pruebas de FacturaService.
 */
public class FacturaServiceTests
{
    /** Repositorio de facturas simulado. */
    private readonly (IFacturaRepository Objeto, Stub<IFacturaRepository> Control) repo = Stub<IFacturaRepository>.Crear();
    /** Movimientos registrados. */
    private readonly AuditoriaEnMemoria auditoria = new();

    /**
     * @brief Crea una línea de factura.
     * @param pro PRO_ID
     * @param cantidad Cantidad
     * @return Línea
     */
    private static FacturaProductoDetalle Linea(decimal pro, decimal cantidad) => new() { ProId = pro, Cantidad = cantidad };

    /**
     * @brief Un empleado siempre factura a su nombre y las líneas del mismo producto se unen.
     */
    [Fact]
    public void Un_empleado_siempre_factura_a_su_nombre_y_las_lineas_repetidas_se_unen()
    {
        repo.Control
            .Cuando(nameof(IFacturaRepository.Insert), _ => 1001m)
            .Cuando(nameof(IFacturaRepository.GetById), _ => new Factura { FacNumFactura = 1001, FacTotal = 11900 });

        new FacturaService(repo.Objeto, Sesiones.Empleado(venId: 7), auditoria)
            .CrearFactura(cliId: 3, venId: 99, "Efectivo", new[] { Linea(1, 2), Linea(1, 3), Linea(2, 1) });

        var args = repo.Control.Llamadas.Single(l => l.Metodo == "Insert").Args;
        Assert.Equal(7m, args[1]);
        var lineas = ((IEnumerable<FacturaProductoDetalle>)args[3]).ToList();
        Assert.Equal(2, lineas.Count);
        Assert.Equal(5, lineas.Single(l => l.ProId == 1).Cantidad);
        Assert.Contains("Venta registrada", auditoria.Acciones);
    }

    /**
     * @brief Un empleado sin vendedor asociado no puede facturar.
     */
    [Fact]
    public void Un_empleado_sin_vendedor_asociado_no_puede_facturar() =>
        Assert.Throws<InvalidOperationException>(() =>
            new FacturaService(repo.Objeto, Sesiones.Empleado(venId: null), auditoria)
                .CrearFactura(3, 0, "Efectivo", new[] { Linea(1, 1) }));

    /**
     * @brief Las cantidades deben ser enteros positivos.
     * @param cantidad Cantidad no válida
     */
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.5)]
    public void Las_cantidades_deben_ser_enteros_positivos(double cantidad) =>
        Assert.Throws<ArgumentException>(() =>
            new FacturaService(repo.Objeto, Sesiones.Admin(), auditoria)
                .CrearFactura(3, 7, "Efectivo", new[] { Linea(1, (decimal)cantidad) }));

    /**
     * @brief El IVA del 19 % se desglosa del total.
     */
    [Fact]
    public void El_iva_se_desglosa_del_total() =>
        Assert.Equal((10000m, 1900m), Factura.DesglosarIva(11900m));
}

/**
 * @brief Pruebas de las clases de infraestructura que no necesitan Oracle.
 */
public class InfraestructuraTests
{
    /**
     * @brief PBKDF2 acepta la contraseña correcta, rechaza otras y usa una sal nueva cada vez.
     */
    [Fact]
    public void Pbkdf2_verifica_la_clave_correcta_y_usa_sal_nueva_cada_vez()
    {
        var hasher = new FarmaciaApp.Infrastructure.Seguridad.HasherPbkdf2();
        var (hash, sal) = hasher.Crear("secreta1");
        Assert.True(hasher.Verificar("secreta1", hash, sal));
        Assert.False(hasher.Verificar("otra", hash, sal));
        Assert.False(hasher.Verificar(null, hash, sal));
        Assert.NotEqual(sal, hasher.Crear("secreta1").Sal);
    }

    /**
     * @brief Sin cadena de conexión la fábrica informa que no está configurada.
     */
    [Fact]
    public void Sin_cadena_de_conexion_la_fabrica_informa_que_no_esta_configurada() =>
        Assert.False(new FarmaciaApp.Infrastructure.Database.OracleConnectionFactory(null).Configurada);
}
