/**
 * @file ProveedoresView.axaml.cs
 * @brief Vista del listado de proveedores.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del listado de proveedores.
     */
    public partial class ProveedoresView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public ProveedoresView()
        {
            InitializeComponent();
            DataContext = new ProveedoresViewModel();
        }
    }
}
