using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.UI.Views;
using System.Windows;
using System.Windows.Controls;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string errorMessage;

        public IRelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            // Command que recibe el PasswordBox como parámetro
            LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin);
        }

        private void ExecuteLogin(PasswordBox passwordBox)
        {
            if (passwordBox == null)
            {
                ErrorMessage = "Error interno: parámetro inválido.";
                return;
            }

            string password = passwordBox.Password;

            // ---- Validación simple ----
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Ingrese usuario y contraseña.";
                return;
            }

            // ---- Credenciales fijas ----
            const string USER = "admin";
            const string PASS = "prueba";

            if (Username == USER && password == PASS)
            {
                OpenMainWindow();
                CloseLoginWindow();
            }
            else
            {
                ErrorMessage = "Usuario o contraseña incorrectos.";
            }
        }

        private void OpenMainWindow()
        {
            var main = new MainWindow();
            main.Show();
        }

        private void CloseLoginWindow()
        {
            // Cerrar únicamente la ventana LoginView
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginView)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
