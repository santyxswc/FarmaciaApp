using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class PromocionesView : UserControl
    {
        public PromocionesView()
        {
            InitializeComponent();
            DataContext = new PromocionesViewModel();
        }
    }
}
