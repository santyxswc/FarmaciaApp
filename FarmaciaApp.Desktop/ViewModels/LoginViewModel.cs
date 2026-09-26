using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public IRelayCommand LoginCommand { get; }

        public LoginViewModel(Window window)
        {
            _window = window;
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Ingrese usuario y contraseña.";
                return;
            }
            const string USER = "admin";
            const string PASS = "prueba";

            if (Username == USER && Password == PASS)
            {
                var main = new MainWindow();
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    desktop.MainWindow = main;
                main.Show();
                _window.Close();
            }
            else
            {
                ErrorMessage = "Usuario o contraseña incorrectos.";
            }
        }
    }
}
