/**
 * @file LoginView.axaml.cs
 * @brief Ventana de inicio de sesión.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Ventana de inicio de sesión.
     */
    public partial class LoginView : Window
    {
        /**
         * @brief Crea la ventana y pone el foco en el usuario.
         */
        public LoginView()
        {
            InitializeComponent();
            Opened += (_, _) => txtUsuario.Focus();
        }
    }
}
