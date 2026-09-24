using FarmaciaApp.UI.ViewModels;
using System.Windows.Controls;
using System.Text;

namespace FarmaciaApp.UI.Views
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
