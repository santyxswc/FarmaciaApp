/**
 * @file MainWindow.axaml.cs
 * @brief Ventana principal con el menu lateral.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Core;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Ventana principal con el menu lateral.
     */
    public partial class MainWindow : Window
    {
        /**
         * @brief Crea la ventana principal.
         *
         * Al cerrarla con la sesión abierta se registra el cierre del turno.
         */
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
            Title = $"Farmacia · {Sesion.Usuario?.Nombre}";

            Closing += (_, _) =>
            {
                if (Sesion.Activa)
                    new UsuarioService().CerrarSesion("ventana cerrada");
            };
        }
    }
}
