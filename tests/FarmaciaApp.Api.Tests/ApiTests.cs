/**
 * @file ApiTests.cs
 * @brief Pruebas de integración de la API.
 * @author Santiago Caicedo
 */
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FarmaciaApp.Api.Endpoints;
using Microsoft.Extensions.Configuration;

namespace FarmaciaApp.Api.Tests;

/**
 * @brief Pruebas de autenticación, permisos y productos sobre la API completa.
 */
public class ApiTests : IClassFixture<FabricaApi>
{
    /** API en memoria. */
    private readonly FabricaApi fabrica;

    /**
     * @brief Crea la clase de pruebas.
     * @param fabrica API en memoria compartida
     */
    public ApiTests(FabricaApi fabrica) => this.fabrica = fabrica;

    /**
     * @brief Cliente HTTP con la sesión de un usuario.
     * @param login Usuario con el que se inicia sesión
     * @return Cliente con el token en la cabecera Authorization
     */
    private async Task<HttpClient> ClienteDe(string login)
    {
        var cliente = fabrica.CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginRequest(login, "clave123"));
        respuesta.EnsureSuccessStatusCode();
        var datos = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", datos.Token);
        return cliente;
    }

    /** Con credenciales correctas el login devuelve un token y el rol. */
    [Fact]
    public async Task Login_correcto_devuelve_token()
    {
        var respuesta = await fabrica.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "clave123"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var datos = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(datos.Token));
        Assert.Equal("Administrador", datos.Usuario.Rol);
    }

    /** Una contraseña incorrecta es 401. */
    [Fact]
    public async Task Login_con_clave_incorrecta_es_401()
    {
        var respuesta = await fabrica.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "otra"));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    /** Una cuenta desactivada no puede entrar (403). */
    [Fact]
    public async Task Login_de_usuario_inactivo_es_403()
    {
        var respuesta = await fabrica.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("inactivo", "clave123"));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    /** Sin token no se accede a los productos. */
    [Fact]
    public async Task Productos_sin_token_es_401()
    {
        var respuesta = await fabrica.CreateClient().GetAsync("/api/productos");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    /** Un empleado puede consultar el catálogo y buscar por nombre. */
    [Fact]
    public async Task Empleado_lista_y_busca_productos()
    {
        var cliente = await ClienteDe("andres");

        var todos = await cliente.GetFromJsonAsync<List<ProductoDto>>("/api/productos");
        var buscados = await cliente.GetFromJsonAsync<List<ProductoDto>>("/api/productos?buscar=ibu");

        Assert.Contains(todos, p => p.Nombre == "Acetaminofén 500 mg");
        Assert.Contains(todos, p => p.Nombre == "Ibuprofeno 400 mg");
        Assert.Single(buscados);
        Assert.True(buscados[0].StockBajo);
    }

    /** Un producto que no existe es 404. */
    [Fact]
    public async Task Producto_inexistente_es_404()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.GetAsync("/api/productos/999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    /** Las reglas de Core niegan al empleado crear productos. */
    [Fact]
    public async Task Empleado_no_puede_crear_productos()
    {
        var cliente = await ClienteDe("andres");

        var respuesta = await cliente.PostAsJsonAsync("/api/productos", new ProductoRequest("Vitamina C", 5000, 10, null));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    /** El administrador crea un producto y la API responde 201 con su ubicación. */
    [Fact]
    public async Task Admin_crea_producto()
    {
        var cliente = await ClienteDe("admin");

        var respuesta = await cliente.PostAsJsonAsync("/api/productos", new ProductoRequest("Vitamina C", 5000, 10, null));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<ProductoDto>();
        Assert.Equal($"/api/productos/{creado.Id}", respuesta.Headers.Location.OriginalString);
    }

    /** Una validación de Core se convierte en 400 con ProblemDetails. */
    [Fact]
    public async Task Precio_invalido_es_400()
    {
        var cliente = await ClienteDe("admin");

        var respuesta = await cliente.PostAsJsonAsync("/api/productos", new ProductoRequest("Gratis", 0, 1, null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType.MediaType);
    }

    /** Un producto con ventas no se elimina: 409. */
    [Fact]
    public async Task Producto_con_ventas_no_se_elimina()
    {
        var cliente = await ClienteDe("admin");

        var respuesta = await cliente.DeleteAsync("/api/productos/2");

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    /** Un token de una cuenta desactivada después del login deja de servir. */
    [Fact]
    public async Task Token_de_cuenta_desactivada_es_401()
    {
        var cliente = await ClienteDe("andres");
        fabrica.Usuarios["andres"].Activo = false;
        try
        {
            var respuesta = await cliente.GetAsync("/api/productos");

            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }
        finally
        {
            fabrica.Usuarios["andres"].Activo = true;
        }
    }

    /** /health/live responde sin tocar la base de datos. */
    [Fact]
    public async Task Health_live_responde_200()
    {
        var respuesta = await fabrica.CreateClient().GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    /** Superado el límite de intentos, el login responde 429. */
    [Fact]
    public async Task Login_supera_el_limite_de_intentos_y_es_429()
    {
        using var limitada = fabrica.WithWebHostBuilder(b => b.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string> { ["RateLimit:LoginPorMinuto"] = "2" })));
        var cliente = limitada.CreateClient();

        var estados = new List<HttpStatusCode>();
        for (int i = 0; i < 3; i++)
            estados.Add((await cliente.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "otra"))).StatusCode);

        Assert.Equal(new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.TooManyRequests }, estados);
    }
}
