/**
 * @file ProductosView.axaml.cs
 * @brief Vista del listado de productos.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del listado de productos.
     */
    public partial class ProductosView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public ProductosView()
        {
            InitializeComponent();
            DataContext = new ProductosViewModel();
        }
    }
}
