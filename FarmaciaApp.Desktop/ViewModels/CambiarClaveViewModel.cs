using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    // Sirve para cambiar la contraseña propia o, si se pasa un usuario, para que el administrador la restablezca
    public partial class CambiarClaveViewModel : ObservableObject
    {
        private readonly Window _window;
        private readonly Usuario _usuario;

        public bool EsPropia => _usuario == null;
        public string Titulo => EsPropia ? "Cambiar mi contraseña" : $"Restablecer contraseña de {_usuario.Login}";
        public string Descripcion => EsPropia
            ? "Por seguridad, primero escribe tu contraseña actual."
            : "El empleado deberá usar esta contraseña en su próximo ingreso.";

        [ObservableProperty]
        private string actual;

        [ObservableProperty]
        private string nueva;

        [ObservableProperty]
        private string confirmacion;

        [ObservableProperty]
        private string errorMessage;

        public IRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public CambiarClaveViewModel(Window window, Usuario usuarioARestablecer = null)
        {
            _window = window;
            _usuario = usuarioARestablecer;
            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        private void Guardar()
        {
            ErrorMessage = null;
            try
            {
                var servicio = new UsuarioService();
                if (EsPropia)
                    servicio.CambiarMiClave(Actual, Nueva, Confirmacion);
                else
                    servicio.RestablecerClave(_usuario.UsuId, Nueva, Confirmacion);
                _window.Close(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
