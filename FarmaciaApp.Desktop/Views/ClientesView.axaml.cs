using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();
            DataContext = new ClientesViewModel();
        }
    }
}
