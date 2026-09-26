using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class FacturasView : UserControl
    {
        public FacturasView()
        {
            InitializeComponent();
            DataContext = new FacturasViewModel();
        }

        // Doble clic en una factura abre su detalle
        private void Grid_DoubleTapped(object sender, Avalonia.Input.TappedEventArgs e) =>
            ((FacturasViewModel)DataContext).VerDetalleCommand.Execute(null);
    }
}
