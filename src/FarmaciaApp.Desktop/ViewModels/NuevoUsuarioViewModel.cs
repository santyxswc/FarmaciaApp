/**
 * @file NuevoUsuarioViewModel.cs
 * @brief Lógica del formulario de nuevo usuario.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Crea una cuenta de empleado o administrador.
     */
    public partial class NuevoUsuarioViewModel : ObservableObject
    {
        private readonly PersonaService _personas;
        private readonly UsuarioService _usuarios;
        private readonly Window _window;

        /** Roles que se pueden asignar. */
        public string[] Roles => UsuarioService.Roles;
        /** Personas que se pueden asociar. */
        public List<Seleccion> Personas { get; }

        /** Usuario. */
        [ObservableProperty]
        private string login;

        /** Rol elegido. */
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AyudaPersona))]
        private string rol = Usuario.RolEmpleado;

        /** Persona asociada. */
        [ObservableProperty]
        private Seleccion personaSeleccionada;

        /** Contraseña. */
        [ObservableProperty]
        private string clave;

        /** Confirmación de la contraseña. */
        [ObservableProperty]
        private string confirmacion;

        /** Error de validación. */
        [ObservableProperty]
        private string errorMessage;

        /** Explicación del campo persona según el rol. */
        public string AyudaPersona => Rol == Usuario.RolEmpleado
            ? "Obligatoria. La persona quedará registrada como vendedor y sus ventas saldrán a su nombre."
            : "Opcional para administradores.";

        /** Comando Crear usuario. */
        public IRelayCommand GuardarCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelarCommand { get; }

        /**
         * @brief Crea el formulario y carga las personas.
         * @param window Ventana del formulario
         * @param personas Reglas de negocio de personas
         * @param usuarios Reglas de negocio de usuarios
         */
        public NuevoUsuarioViewModel(Window window, PersonaService personas, UsuarioService usuarios)
        {
            _personas = personas;
            _usuarios = usuarios;
            _window = window;
            Personas = _personas.ObtenerPersonas()
                .Select(p => new Seleccion { Id = p.PerId, Nombre = p.NombreCompleto })
                .ToList();

            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        /**
         * @brief Crea el usuario y cierra el formulario; si falla, muestra el error.
         */
        private void Guardar()
        {
            ErrorMessage = null;
            try
            {
                _usuarios.CrearUsuario(Login, Clave, Confirmacion, Rol, PersonaSeleccionada?.Id);
                _window.Close(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
