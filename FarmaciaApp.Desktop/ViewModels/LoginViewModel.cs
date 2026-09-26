/**
 * @file LoginViewModel.cs
 * @brief Lógica de la ventana de inicio de sesión.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Views;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Autentica al usuario y abre la ventana principal.
     */
    public partial class LoginViewModel : ObservableObject
    {
        private readonly Window _window;

        /** Usuario escrito. */
        [ObservableProperty]
        private string username;

        /** Contraseña escrita. */
        [ObservableProperty]
        private string password;

        /** Mensaje de error para mostrar en la ventana. */
        [ObservableProperty]
        private string errorMessage;

        /** Indica que se está verificando el ingreso. */
        [ObservableProperty]
        private bool ocupado;

        /** Comando del botón Entrar. */
        public IRelayCommand LoginCommand { get; }

        /**
         * @brief Crea el ViewModel de la ventana de inicio de sesión.
         * @param window Ventana, que se cierra al entrar
         */
        public LoginViewModel(Window window)
        {
            _window = window;
            LoginCommand = new RelayCommand(ExecuteLogin, () => !Ocupado);
        }

        /**
         * @brief Habilita o deshabilita el botón Entrar.
         * @param value Nuevo valor de Ocupado
         */
        partial void OnOcupadoChanged(bool value) => LoginCommand.NotifyCanExecuteChanged();

        /**
         * @brief Verifica la configuración y las credenciales y, si son correctas, abre la ventana principal.
         */
        private void ExecuteLogin()
        {
            ErrorMessage = null;
            if (!DbConfig.Configurada)
            {
                ErrorMessage = "No se encontró la conexión a la base de datos. Copia appsettings.example.json como "
                             + "appsettings.json junto al programa y escribe tus datos de Oracle (ver README).";
                return;
            }
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Ingresa usuario y contraseña.";
                return;
            }

            Ocupado = true;
            try
            {
                var usuario = new UsuarioService().IniciarSesion(Username, Password);
                if (usuario == null)
                {
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    Password = "";
                    return;
                }

                var main = new MainWindow();
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    desktop.MainWindow = main;
                main.Show();
                _window.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex is InvalidOperationException
                    ? ex.Message
                    : $"No se pudo conectar con la base de datos: {ex.Message}";
            }
            finally
            {
                Ocupado = false;
            }
        }
    }
}
