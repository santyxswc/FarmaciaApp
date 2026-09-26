/**
 * @file CambiarClaveViewModel.cs
 * @brief Lógica del cambio de contraseña.
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
     * @brief Cambia la contraseña propia o, si se indica un usuario, la restablece.
     *
     * Restablecer la contraseña de otro usuario es solo para el administrador.
     */
    public partial class CambiarClaveViewModel : ObservableObject
    {
        private readonly Window _window;
        private readonly Usuario _usuario;

        /** Indica si el usuario cambia su propia contraseña (pide la actual). */
        public bool EsPropia => _usuario == null;
        /** Título de la ventana. */
        public string Titulo => EsPropia ? "Cambiar mi contraseña" : $"Restablecer contraseña de {_usuario.Login}";
        /** Texto de ayuda bajo el título. */
        public string Descripcion => EsPropia
            ? "Por seguridad, primero escribe tu contraseña actual."
            : "El empleado deberá usar esta contraseña en su próximo ingreso.";

        /** Contraseña actual. */
        [ObservableProperty]
        private string actual;

        /** Contraseña nueva. */
        [ObservableProperty]
        private string nueva;

        /** Confirmación de la contraseña nueva. */
        [ObservableProperty]
        private string confirmacion;

        /** Error de validación. */
        [ObservableProperty]
        private string errorMessage;

        /** Comando Guardar. */
        public IRelayCommand GuardarCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelarCommand { get; }

        /**
         * @brief Crea el formulario.
         * @param window Ventana del formulario
         * @param usuarioARestablecer Usuario al que se le restablece la contraseña; null para la propia
         */
        public CambiarClaveViewModel(Window window, Usuario usuarioARestablecer = null)
        {
            _window = window;
            _usuario = usuarioARestablecer;
            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        /**
         * @brief Cambia o restablece la contraseña y cierra el formulario.
         */
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
