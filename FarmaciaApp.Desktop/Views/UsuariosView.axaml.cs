using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class UsuariosView : UserControl
    {
        public UsuariosView()
        {
            InitializeComponent();
            DataContext = new UsuariosViewModel();
        }
    }
}
