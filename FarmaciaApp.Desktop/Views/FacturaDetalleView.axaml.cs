using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FarmaciaApp.Desktop.Views
{
    public partial class FacturaDetalleView : Window
    {
        public FacturaDetalleView()
        {
            InitializeComponent();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e) => Close(false);
    }
}
