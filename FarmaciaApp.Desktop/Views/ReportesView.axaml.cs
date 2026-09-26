/**
 * @file ReportesView.axaml.cs
 * @brief Vista de los reportes de ventas.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista de los reportes de ventas.
     */
    public partial class ReportesView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public ReportesView()
        {
            InitializeComponent();
            DataContext = new ReportesViewModel();
        }
    }
}
