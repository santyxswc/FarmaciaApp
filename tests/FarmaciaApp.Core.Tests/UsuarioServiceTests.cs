/**
 * @file UsuarioServiceTests.cs
 * @brief Pruebas del servicio de usuarios.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Core.Tests;

/**
 * @brief Pruebas de inicio de sesión, turnos y administración de usuarios.
 */
public class UsuarioServiceTests
{
    /** Repositorio de usuarios simulado. */
    private readonly (IUsuarioRepository Objeto, Stub<IUsuarioRepository> Control) usuarios = Stub<IUsuarioRepository>.Crear();
    /** Repositorio de personas simulado. */
    private readonly (IPersonaRepository Objeto, Stub<IPersonaRepository> Control) personas = Stub<IPersonaRepository>.Crear();
    /** Movimientos registrados. */
    private readonly AuditoriaEnMemoria auditoria = new();

    /**
     * @brief Crea el servicio con los dobles.
     * @param sesion Sesión del turno
     * @return Servicio de usuarios
     */
    private UsuarioService Servicio(ISesionUsuario sesion) =>
        new(usuarios.Objeto,
            new PersonaService(personas.Objeto, usuarios.Objeto, sesion, auditoria),
            sesion, auditoria, new HasherFalso());

    /**
     * @brief Usuario guardado con la contraseña "secreta1".
     * @param activo Si el usuario está activo
     * @return Usuario
     */
    private static Usuario Registrado(bool activo = true) =>
        new() { UsuId = 10, Login = "ana", Hash = "h:secreta1", Sal = "sal", Rol = Usuario.RolEmpleado, Activo = activo, PerId = 3 };

    /**
     * @brief Iniciar sesión abre el turno con el vendedor asociado y lo registra.
     */
    [Fact]
    public void Iniciar_sesion_abre_el_turno_y_lo_audita()
    {
        usuarios.Control
            .Cuando(nameof(IUsuarioRepository.GetByLogin), _ => Registrado())
            .Cuando(nameof(IUsuarioRepository.RegistrarIngreso), _ => null)
            .Cuando(nameof(IUsuarioRepository.GetVenIdPorPersona), _ => (decimal?)7);
        var sesion = new SesionUsuario();

        var usuario = Servicio(sesion).IniciarSesion("  ANA ", "secreta1");

        Assert.NotNull(usuario);
        Assert.True(sesion.Activa);
        Assert.Equal(7, sesion.VenId);
        Assert.Contains("Inicio de sesión", auditoria.Acciones);
        Assert.Equal("ana", usuarios.Control.Llamadas.First(l => l.Metodo == "GetByLogin").Args[0]);
    }

    /**
     * @brief Una contraseña incorrecta no abre sesión y queda registrada como intento fallido.
     */
    [Fact]
    public void Una_clave_incorrecta_no_abre_sesion_y_queda_registrada()
    {
        usuarios.Control.Cuando(nameof(IUsuarioRepository.GetByLogin), _ => Registrado());
        var sesion = new SesionUsuario();

        Assert.Null(Servicio(sesion).IniciarSesion("ana", "otra"));
        Assert.False(sesion.Activa);
        Assert.Contains("Inicio de sesión fallido", auditoria.Acciones);
    }

    /**
     * @brief Un usuario desactivado no puede entrar.
     */
    [Fact]
    public void Un_usuario_desactivado_no_puede_entrar()
    {
        usuarios.Control.Cuando(nameof(IUsuarioRepository.GetByLogin), _ => Registrado(activo: false));
        Assert.Throws<InvalidOperationException>(() => Servicio(new SesionUsuario()).IniciarSesion("ana", "secreta1"));
    }

    /**
     * @brief Solo el administrador crea usuarios.
     */
    [Fact]
    public void Solo_el_administrador_crea_usuarios()
    {
        var error = Assert.Throws<UnauthorizedAccessException>(() =>
            Servicio(Sesiones.Empleado()).CrearUsuario("nuevo", "clave123", "clave123", Usuario.RolAdministrador, null));
        Assert.Contains("administrador", error.Message);
        Assert.Equal(0, usuarios.Control.Veces(nameof(IUsuarioRepository.Insert)));
    }

    /**
     * @brief Rechaza login corto, contraseña corta, contraseñas distintas y empleado sin persona.
     * @param login Login
     * @param clave Contraseña
     * @param confirmacion Confirmación
     * @param rol Rol
     */
    [Theory]
    [InlineData("ab", "clave123", "clave123", "Administrador")]
    [InlineData("nuevo", "123", "123", "Administrador")]
    [InlineData("nuevo", "clave123", "otra1234", "Administrador")]
    [InlineData("nuevo", "clave123", "clave123", "Empleado")]
    public void Crear_usuario_valida_los_datos(string login, string clave, string confirmacion, string rol) =>
        Assert.Throws<ArgumentException>(() => Servicio(Sesiones.Admin()).CrearUsuario(login, clave, confirmacion, rol, null));

    /**
     * @brief Crear un usuario guarda el login normalizado y el hash, no la contraseña.
     */
    [Fact]
    public void Crear_usuario_guarda_el_hash_y_no_la_clave()
    {
        usuarios.Control
            .Cuando(nameof(IUsuarioRepository.GetByLogin), _ => null)
            .Cuando(nameof(IUsuarioRepository.Insert), _ => 99m);

        decimal id = Servicio(Sesiones.Admin()).CrearUsuario("Nuevo.Admin", "clave123", "clave123", Usuario.RolAdministrador, null);

        var insert = usuarios.Control.Llamadas.Single(l => l.Metodo == "Insert").Args;
        Assert.Equal(99m, id);
        Assert.Equal("nuevo.admin", insert[0]);
        Assert.Equal("h:clave123", insert[1]);
        Assert.Contains("Usuario creado", auditoria.Acciones);
    }

    /**
     * @brief No se puede desactivar al último administrador activo.
     */
    [Fact]
    public void No_se_puede_desactivar_al_ultimo_administrador()
    {
        usuarios.Control
            .Cuando(nameof(IUsuarioRepository.GetById), _ => new Usuario { UsuId = 50, Rol = Usuario.RolAdministrador, Activo = true })
            .Cuando(nameof(IUsuarioRepository.CountAdminsActivos), _ => 1);

        Assert.Throws<InvalidOperationException>(() => Servicio(Sesiones.Admin()).CambiarEstado(50, activo: false));
    }

    /**
     * @brief Cerrar sesión registra el turno y la cierra.
     */
    [Fact]
    public void Cerrar_sesion_audita_el_turno_y_la_cierra()
    {
        var sesion = Sesiones.Empleado();
        Servicio(sesion).CerrarSesion("ventana cerrada");
        Assert.False(sesion.Activa);
        Assert.Contains("Cierre de sesión", auditoria.Acciones);
    }
}
