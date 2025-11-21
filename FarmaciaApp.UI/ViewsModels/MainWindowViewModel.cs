using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.UI.Services;
using FarmaciaApp.UI.Views;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        public NavigationService Navigation { get; set; }

        public IRelayCommand GoHomeCommand { get; }
        public IRelayCommand GoProductosCommand { get; }
        public IRelayCommand GoClientesCommand { get; }
        public IRelayCommand GoFacturasCommand { get; }
        public IRelayCommand GoReclamosCommand { get; }

        public MainWindowViewModel()
        {
            Navigation = new NavigationService();

            GoHomeCommand = new RelayCommand(OpenHome);
            GoProductosCommand = new RelayCommand(OpenProductos);
            GoClientesCommand = new RelayCommand(OpenClientes);
            GoFacturasCommand = new RelayCommand(OpenFacturas);
            GoReclamosCommand = new RelayCommand(OpenReclamos);

            OpenHome(); // vista inicial
        }

        private void OpenHome()
        {
            Navigation.Navigate(new HomeView());
        }

        private void OpenProductos()
        {
            Navigation.Navigate(new ProductosView());
        }

        private void OpenClientes()
        {
            Navigation.Navigate(new ClientesView());   
        }

        private void OpenFacturas()
        {
            Navigation.Navigate(new FacturasView());   
        }

        private void OpenReclamos()
        {
            Navigation.Navigate(new ReclamosView());   
        }
    }
}
