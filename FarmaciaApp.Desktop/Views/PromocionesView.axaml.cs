/**
 * @file PromocionesView.axaml.cs
 * @brief Vista del listado de promociones.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del listado de promociones.
     */
    public partial class PromocionesView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public PromocionesView()
        {
            InitializeComponent();
            DataContext = new PromocionesViewModel();
        }
    }
}
