/**
 * @file MovimientosView.axaml.cs
 * @brief Vista del registro de movimientos.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del registro de movimientos.
     */
    public partial class MovimientosView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public MovimientosView()
        {
            InitializeComponent();
            DataContext = new MovimientosViewModel();
        }
    }
}
