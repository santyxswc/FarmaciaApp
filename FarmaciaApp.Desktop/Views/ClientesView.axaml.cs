/**
 * @file ClientesView.axaml.cs
 * @brief Vista del listado de clientes.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del listado de clientes.
     */
    public partial class ClientesView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public ClientesView()
        {
            InitializeComponent();
            DataContext = new ClientesViewModel();
        }
    }
}
