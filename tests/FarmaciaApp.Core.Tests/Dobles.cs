/**
 * @file Dobles.cs
 * @brief Dobles de prueba de los contratos de Core.
 * @author Santiago Caicedo
 */
using System.Reflection;
using FarmaciaApp.Core.Abstractions;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Sesion;

namespace FarmaciaApp.Core.Tests;

/**
 * @brief Doble de cualquier interfaz: cada prueba configura solo los métodos que usa y consulta las llamadas.
 *
 * Un método no configurado lanza una excepción.
 */
public class Stub<T> : DispatchProxy where T : class
{
    /** Respuesta de cada método configurado, por nombre. */
    private readonly Dictionary<string, Func<object[], object>> respuestas = new();
    /** Llamadas recibidas, en orden. */
    public List<(string Metodo, object[] Args)> Llamadas { get; } = new();

    /**
     * @brief Crea el doble.
     * @return La interfaz para el código bajo prueba y el control para configurarla
     */
    public static (T Objeto, Stub<T> Control) Crear()
    {
        var objeto = Create<T, Stub<T>>();
        return (objeto, (Stub<T>)(object)objeto);
    }

    /**
     * @brief Configura la respuesta de un método.
     * @param metodo Nombre del método
     * @param respuesta Función que recibe los argumentos y devuelve el resultado
     * @return El mismo doble, para encadenar
     */
    public Stub<T> Cuando(string metodo, Func<object[], object> respuesta)
    {
        respuestas[metodo] = respuesta;
        return this;
    }

    /**
     * @brief Cuenta las llamadas a un método.
     * @param metodo Nombre del método
     * @return Número de llamadas
     */
    public int Veces(string metodo) => Llamadas.Count(l => l.Metodo == metodo);

    /**
     * @brief Registra la llamada y devuelve la respuesta configurada.
     * @param metodo Método invocado
     * @param args Argumentos
     * @return Resultado configurado
     * @exception InvalidOperationException Si el método no está configurado
     */
    protected override object Invoke(MethodInfo metodo, object[] args)
    {
        Llamadas.Add((metodo.Name, args));
        if (respuestas.TryGetValue(metodo.Name, out var respuesta))
            return respuesta(args);
        throw new InvalidOperationException($"{typeof(T).Name}.{metodo.Name} no está configurado en la prueba.");
    }
}

/**
 * @brief Auditoría que guarda las acciones en una lista.
 */
public sealed class AuditoriaEnMemoria : IAuditoria
{
    /** Acciones registradas. */
    public List<string> Acciones { get; } = new();
    /**
     * @brief Registra una acción.
     * @param accion Acción
     * @param detalle Detalle (no se guarda)
     */
    public void Registrar(string accion, string detalle = null) => Acciones.Add(accion);
}

/**
 * @brief Hasher sin el coste de PBKDF2 para las pruebas de reglas.
 */
public sealed class HasherFalso : IHasherClaves
{
    /**
     * @brief Genera un hash predecible.
     * @param clave Contraseña
     * @return Hash "h:clave" y una sal fija
     */
    public (string Hash, string Sal) Crear(string clave) => ("h:" + clave, "sal");
    /**
     * @brief Comprueba la contraseña contra el hash predecible.
     * @param clave Contraseña
     * @param hash Hash guardado
     * @param sal Sal (no se usa)
     * @return true si coincide
     */
    public bool Verificar(string clave, string hash, string sal) => hash == "h:" + clave;
}

/**
 * @brief Sesiones de usuario ya iniciadas.
 */
public static class Sesiones
{
    /**
     * @brief Sesión de un administrador.
     * @return Sesión iniciada
     */
    public static SesionUsuario Admin()
    {
        var sesion = new SesionUsuario();
        sesion.Iniciar(new Usuario { UsuId = 1, Login = "admin", Rol = Usuario.RolAdministrador, Activo = true }, null);
        return sesion;
    }

    /**
     * @brief Sesión de un empleado.
     * @param venId VEN_ID del empleado, o null si no es vendedor
     * @return Sesión iniciada
     */
    public static SesionUsuario Empleado(decimal? venId = 7)
    {
        var sesion = new SesionUsuario();
        sesion.Iniciar(new Usuario { UsuId = 2, Login = "empleado", Rol = Usuario.RolEmpleado, Activo = true, PerId = 5 }, venId);
        return sesion;
    }
}
