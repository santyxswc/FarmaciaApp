using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Views;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly Window _window;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool ocupado;

        public IRelayCommand LoginCommand { get; }

        public LoginViewModel(Window window)
        {
            _window = window;
            LoginCommand = new RelayCommand(ExecuteLogin, () => !Ocupado);
        }

        partial void OnOcupadoChanged(bool value) => LoginCommand.NotifyCanExecuteChanged();

        private void ExecuteLogin()
        {
            ErrorMessage = null;
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
