/**
 * @file FacturasView.axaml.cs
 * @brief Vista del listado de facturas.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del listado de facturas.
     */
    public partial class FacturasView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public FacturasView()
        {
            InitializeComponent();
            DataContext = new FacturasViewModel();
        }

        /**
         * @brief Abre el detalle de la factura con doble clic.
         * @param sender Tabla
         * @param e Evento
         */
        private void Grid_DoubleTapped(object sender, Avalonia.Input.TappedEventArgs e) =>
            ((FacturasViewModel)DataContext).VerDetalleCommand.Execute(null);
    }
}
