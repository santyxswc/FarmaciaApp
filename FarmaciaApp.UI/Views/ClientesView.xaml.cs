using FarmaciaApp.UI.ViewModels; // Necesitas este 'using' para acceder al ViewModel
using System.Windows.Controls;
using System.Text;

namespace FarmaciaApp.UI.Views
{
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();

            // ¡Línea CRÍTICA! Asigna el ViewModel como contexto de datos.
            DataContext = new ClientesViewModel();
        }
    }
}
