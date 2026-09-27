/**
 * @file ReclamosView.axaml.cs
 * @brief Vista del listado de reclamos.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del listado de reclamos.
     */
    public partial class ReclamosView : UserControl
    {
        /**
         * @brief Crea la vista; FabricaVistas le asigna su ViewModel.
         */
        public ReclamosView()
        {
            InitializeComponent();
        }
    }
}
