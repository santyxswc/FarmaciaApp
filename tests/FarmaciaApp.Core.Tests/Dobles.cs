using System.Reflection;
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Core.Tests;

/// Doble genérico de cualquier interfaz: cada prueba define solo los métodos que usa y
/// registra las llamadas. Un método no configurado falla, así la prueba no depende de nada implícito.
public class Stub<T> : DispatchProxy where T : class
{
    private readonly Dictionary<string, Func<object[], object>> respuestas = new();
    public List<(string Metodo, object[] Args)> Llamadas { get; } = new();

    public static (T Objeto, Stub<T> Control) Crear()
    {
        var objeto = Create<T, Stub<T>>();
        return (objeto, (Stub<T>)(object)objeto);
    }

    public Stub<T> Cuando(string metodo, Func<object[], object> respuesta)
    {
        respuestas[metodo] = respuesta;
        return this;
    }

    public int Veces(string metodo) => Llamadas.Count(l => l.Metodo == metodo);

    protected override object Invoke(MethodInfo metodo, object[] args)
    {
        Llamadas.Add((metodo.Name, args));
        if (respuestas.TryGetValue(metodo.Name, out var respuesta))
            return respuesta(args);
        throw new InvalidOperationException($"{typeof(T).Name}.{metodo.Name} no está configurado en la prueba.");
    }
}

/// Auditoría en memoria: permite comprobar qué movimientos registra cada servicio.
public sealed class AuditoriaEnMemoria : IAuditoria
{
    public List<string> Acciones { get; } = new();
    public void Registrar(string accion, string detalle = null) => Acciones.Add(accion);
}

/// Hasher sin coste de PBKDF2 para las pruebas de reglas.
public sealed class HasherFalso : IHasherClaves
{
    public (string Hash, string Sal) Crear(string clave) => ("h:" + clave, "sal");
    public bool Verificar(string clave, string hash, string sal) => hash == "h:" + clave;
}

public static class Sesiones
{
    public static SesionUsuario Admin()
    {
        var sesion = new SesionUsuario();
        sesion.Iniciar(new Usuario { UsuId = 1, Login = "admin", Rol = Usuario.RolAdministrador, Activo = true }, null);
        return sesion;
    }

    public static SesionUsuario Empleado(decimal? venId = 7)
    {
        var sesion = new SesionUsuario();
        sesion.Iniciar(new Usuario { UsuId = 2, Login = "empleado", Rol = Usuario.RolEmpleado, Activo = true, PerId = 5 }, venId);
        return sesion;
    }
}
