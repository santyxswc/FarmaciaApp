using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.UI.Helpers;
using FarmaciaApp.UI.Views;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private NavigationService navigation;
        public IRelayCommand GoHomeCommand { get; }
        public IRelayCommand GoProductosCommand { get; }
        public IRelayCommand GoClientesCommand { get; }
        public IRelayCommand GoFacturasCommand { get; }
        public IRelayCommand GoReclamosCommand { get; }
        public IRelayCommand GoPersonasCommand { get; }
        public IRelayCommand GoProveedoresCommand { get; }
        public IRelayCommand GoPromocionesCommand { get; }

        public MainWindowViewModel()
        {
            Navigation = new NavigationService();
            GoHomeCommand = new RelayCommand(NavigateToHome);
            GoProductosCommand = new RelayCommand(NavigateToProductos);
            GoClientesCommand = new RelayCommand(NavigateToClientes);
            GoFacturasCommand = new RelayCommand(NavigateToFacturas);
            GoReclamosCommand = new RelayCommand(NavigateToReclamos);
            GoPersonasCommand = new RelayCommand(NavigateToPersonas);
            GoProveedoresCommand = new RelayCommand(NavigateToProveedores);
            GoPromocionesCommand = new RelayCommand(NavigateToPromociones);
            NavigateToHome();
        }
        private void NavigateToHome() => Navigation.NavigateTo(new HomeView());
        private void NavigateToProductos() => Navigation.NavigateTo(new ProductosView());
        private void NavigateToClientes() => Navigation.NavigateTo(new ClientesView());
        private void NavigateToFacturas() => Navigation.NavigateTo(new FacturasView());
        private void NavigateToReclamos() => Navigation.NavigateTo(new ReclamosView());
        private void NavigateToPersonas() => Navigation.NavigateTo(new PersonasView());
        private void NavigateToProveedores() => Navigation.NavigateTo(new ProveedoresView());
        private void NavigateToPromociones() => Navigation.NavigateTo(new PromocionesView());
    }
}