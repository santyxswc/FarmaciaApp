using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class NuevoUsuarioViewModel : ObservableObject
    {
        private readonly Window _window;

        public string[] Roles => UsuarioService.Roles;
        public List<Seleccion> Personas { get; }

        [ObservableProperty]
        private string login;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AyudaPersona))]
        private string rol = Usuario.RolEmpleado;

        [ObservableProperty]
        private Seleccion personaSeleccionada;

        [ObservableProperty]
        private string clave;

        [ObservableProperty]
        private string confirmacion;

        [ObservableProperty]
        private string errorMessage;

        public string AyudaPersona => Rol == Usuario.RolEmpleado
            ? "Obligatoria. La persona quedará registrada como vendedor y sus ventas saldrán a su nombre."
            : "Opcional para administradores.";

        public IRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public NuevoUsuarioViewModel(Window window)
        {
            _window = window;
            Personas = new PersonaService().ObtenerPersonas()
                .Select(p => new Seleccion { Id = p.PerId, Nombre = p.NombreCompleto })
                .ToList();

            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        private void Guardar()
        {
            ErrorMessage = null;
            try
            {
                new UsuarioService().CrearUsuario(Login, Clave, Confirmacion, Rol, PersonaSeleccionada?.Id);
                _window.Close(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
