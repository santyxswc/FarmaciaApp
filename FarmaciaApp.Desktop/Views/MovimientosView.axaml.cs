using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class MovimientosView : UserControl
    {
        public MovimientosView()
        {
            InitializeComponent();
            DataContext = new MovimientosViewModel();
        }
    }
}
