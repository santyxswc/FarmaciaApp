/**
 * @file Program.cs
 * @brief Punto de entrada de la API REST.
 * @author Santiago Caicedo
 *
 * Expone las reglas de Core por HTTP con autenticación JWT. Comparte Core e Infrastructure
 * con la aplicación de escritorio.
 */
using System.Globalization;
using System.Threading.RateLimiting;
using FarmaciaApp.Api;
using FarmaciaApp.Api.Endpoints;
using FarmaciaApp.Api.Errores;
using FarmaciaApp.Api.Salud;
using FarmaciaApp.Api.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var cultura = FarmaciaApp.Core.Formato.Cultura;
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();

builder.Services.AgregarFarmacia(builder.Configuration.GetConnectionString("OracleConnection"));

builder.Services.AddOptions<OpcionesJwt>()
    .BindConfiguration("Jwt")
    .Validate(o => o.EsValida(), $"Jwt:Key debe tener al menos {OpcionesJwt.LargoMinimoClave} caracteres.")
    .ValidateOnStart();
builder.Services.AddSingleton<EmisorTokens>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<OpcionesJwt>>((opciones, jwt) =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = jwt.Value.ParametrosDeValidacion();
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(_ => { });
builder.Services.AddOptions<RateLimiterOptions>().Configure<IConfiguration>((opciones, configuracion) =>
{
    int intentos = configuracion.GetValue("RateLimit:LoginPorMinuto", 10);
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy("login", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = intentos, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErrores>();

builder.Services.AddHealthChecks().AddCheck<OracleHealthCheck>("oracle", tags: new[] { "ready" });
builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer<EsquemaBearer>());

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<SesionPorPeticionMiddleware>();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference(opciones => opciones.WithTitle("FarmaciaApp API"));
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

app.MapAuth();
app.MapProductos();
app.MapClientes();
app.MapPersonas();
app.MapProveedores();
app.MapPromociones();
app.MapReclamos();
app.MapFacturas();
app.MapReportes();
app.MapMovimientos();
app.MapUsuarios();

app.Run();

/** Expuesta para las pruebas de integración. */
public partial class Program;
