/**
 * @file MainWindow.axaml.cs
 * @brief Ventana principal con el menu lateral.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Ventana principal con el menu lateral.
     */
    public partial class MainWindow : Window
    {
        /**
         * @brief Crea la ventana; FabricaVistas le asigna su ViewModel.
         *
         * Al cerrar la ventana se cierra el turno del usuario.
         */
        public MainWindow()
        {
            InitializeComponent();
            Closing += (_, _) => (DataContext as MainWindowViewModel)?.CerrarTurnoPorVentana();
        }
    }
}
