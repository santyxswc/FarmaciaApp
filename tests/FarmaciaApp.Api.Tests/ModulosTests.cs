/**
 * @file ModulosTests.cs
 * @brief Pruebas de integración de los demás módulos de la API.
 * @author Santiago Caicedo
 */
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FarmaciaApp.Api.Endpoints;
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Api.Tests;

/**
 * @brief Pruebas de clientes, ventas, reportes, movimientos, reclamos y usuarios.
 */
public class ModulosTests : IClassFixture<FabricaApi>
{
    /** API en memoria. */
    private readonly FabricaApi fabrica;

    /**
     * @brief Crea la clase de pruebas.
     * @param fabrica API en memoria compartida
     */
    public ModulosTests(FabricaApi fabrica) => this.fabrica = fabrica;

    /**
     * @brief Cliente HTTP con la sesión de un usuario.
     * @param login Usuario con el que se inicia sesión
     * @return Cliente con el token en la cabecera Authorization
     */
    private async Task<HttpClient> ClienteDe(string login)
    {
        var cliente = fabrica.CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginRequest(login, "clave123"));
        var datos = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", datos.Token);
        return cliente;
    }

    /** Un empleado registra clientes y la API responde 201. */
    [Fact]
    public async Task Empleado_crea_cliente()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.PostAsJsonAsync("/api/clientes", new ClienteRequest("Pedro", "Ruiz", null, null, "pedro@correo.com"));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    /** Un email sin arroba es 400. */
    [Fact]
    public async Task Cliente_con_email_invalido_es_400()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.PostAsJsonAsync("/api/clientes", new ClienteRequest("Pedro", "Ruiz", null, null, "sin-arroba"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /** Eliminar clientes es del administrador. */
    [Fact]
    public async Task Empleado_no_elimina_clientes()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.DeleteAsync("/api/clientes/10");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    /** Un cliente con facturas no se elimina, ni siquiera el administrador. */
    [Fact]
    public async Task Cliente_con_facturas_no_se_elimina()
    {
        var cliente = await ClienteDe("admin");

        var respuesta = await cliente.DeleteAsync("/api/clientes/10");

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    /** La venta de un empleado sale a su nombre aunque pida otro vendedor. */
    [Fact]
    public async Task Venta_de_empleado_usa_su_propio_vendedor()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.PostAsJsonAsync("/api/ventas",
            new VentaRequest(10, 99, "Efectivo", new() { new LineaVentaRequest(1, 2) }));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var factura = await respuesta.Content.ReadFromJsonAsync<FacturaDto>();
        Assert.Equal(1001, factura.Numero);
        Assert.Single(factura.Items);
        var insercion = fabrica.ControlFacturas.Llamadas.Last(l => l.Metodo == "Insert");
        Assert.Equal(7m, insercion.Args[1]);
    }

    /** Una venta sin productos es 400. */
    [Fact]
    public async Task Venta_sin_productos_es_400()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.PostAsJsonAsync("/api/ventas", new VentaRequest(10, null, "Efectivo", new()));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /** El detalle de una factura trae sus líneas. */
    [Fact]
    public async Task Detalle_de_factura_incluye_lineas()
    {
        var cliente = await ClienteDe("andres");

        var factura = await cliente.GetFromJsonAsync<FacturaDto>("/api/facturas/1001");

        Assert.Equal(11900, factura.Total);
        Assert.Equal("Acetaminofén 500 mg", factura.Items[0].Producto);
    }

    /** Los reportes son del administrador. */
    [Fact]
    public async Task Reportes_son_solo_para_administrador()
    {
        var empleado = await ClienteDe("andres");
        var admin = await ClienteDe("admin");

        Assert.Equal(HttpStatusCode.Forbidden, (await empleado.GetAsync("/api/reportes/resumen")).StatusCode);
        var resumen = await admin.GetFromJsonAsync<ResumenVentasDto>("/api/reportes/resumen?desde=2026-10-01&hasta=2026-10-07");
        Assert.Equal(25000, resumen.TicketPromedio);
    }

    /** Un periodo al revés es 400. */
    [Fact]
    public async Task Reporte_con_periodo_invertido_es_400()
    {
        var admin = await ClienteDe("admin");

        var respuesta = await admin.GetAsync("/api/reportes/resumen?desde=2026-10-07&hasta=2026-10-01");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /** Los movimientos son del administrador. */
    [Fact]
    public async Task Movimientos_son_solo_para_administrador()
    {
        var empleado = await ClienteDe("andres");
        var admin = await ClienteDe("admin");

        Assert.Equal(HttpStatusCode.Forbidden, (await empleado.GetAsync("/api/movimientos")).StatusCode);
        var movimientos = await admin.GetFromJsonAsync<List<MovimientoDto>>("/api/movimientos");
        Assert.Single(movimientos);
    }

    /** Un reclamo sin descripción es 400, y los estados se pueden consultar. */
    [Fact]
    public async Task Reclamos_validan_y_publican_sus_estados()
    {
        var cliente = await ClienteDe("andres");

        var invalido = await cliente.PostAsJsonAsync("/api/reclamos", new ReclamoRequest(1001, " ", null));
        var estados = await cliente.GetFromJsonAsync<string[]>("/api/reclamos/estados");

        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
        Assert.NotEmpty(estados);
    }

    /** La lista de usuarios es del administrador y nunca expone el hash. */
    [Fact]
    public async Task Lista_de_usuarios_no_expone_el_hash()
    {
        var empleado = await ClienteDe("andres");
        var admin = await ClienteDe("admin");

        Assert.Equal(HttpStatusCode.Forbidden, (await empleado.GetAsync("/api/usuarios")).StatusCode);
        var texto = await admin.GetStringAsync("/api/usuarios");
        Assert.Contains("\"login\":\"admin\"", texto);
        Assert.DoesNotContain("h:clave123", texto);
        Assert.DoesNotContain("hash", texto, StringComparison.OrdinalIgnoreCase);
    }

    /** Un login repetido es 409; contraseñas distintas, 400. */
    [Fact]
    public async Task Crear_usuario_valida_login_y_clave()
    {
        var admin = await ClienteDe("admin");

        var repetido = await admin.PostAsJsonAsync("/api/usuarios",
            new CrearUsuarioRequest("admin", "secreta1", "secreta1", Usuario.RolAdministrador, null));
        var distintas = await admin.PostAsJsonAsync("/api/usuarios",
            new CrearUsuarioRequest("nuevo", "secreta1", "otra123", Usuario.RolAdministrador, null));

        Assert.Equal(HttpStatusCode.Conflict, repetido.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, distintas.StatusCode);
    }

    /** Con la contraseña actual equivocada no se puede cambiar. */
    [Fact]
    public async Task Cambiar_clave_exige_la_actual()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.PutAsJsonAsync("/api/auth/clave", new CambiarClaveRequest("equivocada", "nueva123", "nueva123"));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    /** El documento OpenAPI publica todos los módulos. */
    [Fact]
    public async Task OpenApi_lista_los_modulos()
    {
        var texto = await fabrica.CreateClient().GetStringAsync("/openapi/v1.json");

        foreach (var ruta in new[] { "/api/clientes", "/api/personas", "/api/proveedores", "/api/promociones",
                                     "/api/reclamos", "/api/facturas", "/api/ventas", "/api/reportes/resumen",
                                     "/api/movimientos", "/api/usuarios" })
            Assert.Contains(ruta, texto);
    }

    /** La raíz sirve la interfaz web con sus cabeceras de seguridad. */
    [Fact]
    public async Task La_raiz_sirve_la_interfaz_web()
    {
        var respuesta = await fabrica.CreateClient().GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("text/html", respuesta.Content.Headers.ContentType.MediaType);
        Assert.Contains("FarmaciaApp", await respuesta.Content.ReadAsStringAsync());
        Assert.Contains("default-src 'self'", respuesta.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("nosniff", respuesta.Headers.GetValues("X-Content-Type-Options").Single());
    }

    /** Los módulos de JavaScript y los estilos se publican. */
    [Theory]
    [InlineData("/js/app.js")]
    [InlineData("/js/vistas/venta.js")]
    [InlineData("/js/vistas/clientes.js")]
    [InlineData("/js/vistas/reclamos.js")]
    [InlineData("/js/vistas/promociones.js")]
    [InlineData("/js/vistas/personas.js")]
    [InlineData("/js/vistas/usuarios.js")]
    [InlineData("/css/app.css")]
    public async Task Los_recursos_estaticos_se_publican(string ruta)
    {
        var respuesta = await fabrica.CreateClient().GetAsync(ruta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    /** La documentación interactiva sigue disponible fuera de la política de contenido. */
    [Fact]
    public async Task Scalar_no_lleva_la_politica_de_contenido()
    {
        var respuesta = await fabrica.CreateClient().GetAsync("/scalar/v1");

        Assert.False(respuesta.Headers.Contains("Content-Security-Policy"));
    }

    /** Los métodos de pago se consultan desde la API. */
    [Fact]
    public async Task Metodos_de_pago_se_publican()
    {
        var cliente = await ClienteDe("andres");

        var metodos = await cliente.GetFromJsonAsync<string[]>("/api/ventas/metodos-pago");

        Assert.Contains("Efectivo", metodos);
    }
}
