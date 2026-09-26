using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Desktop.Views;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private Control currentView;

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
            GoHomeCommand = new RelayCommand(() => CurrentView = new HomeView());
            GoProductosCommand = new RelayCommand(() => CurrentView = new ProductosView());
            GoClientesCommand = new RelayCommand(() => CurrentView = new ClientesView());
            GoFacturasCommand = new RelayCommand(() => CurrentView = new FacturasView());
            GoReclamosCommand = new RelayCommand(() => CurrentView = new ReclamosView());
            GoPersonasCommand = new RelayCommand(() => CurrentView = new PersonasView());
            GoProveedoresCommand = new RelayCommand(() => CurrentView = new ProveedoresView());
            GoPromocionesCommand = new RelayCommand(() => CurrentView = new PromocionesView());
            CurrentView = new HomeView();
        }
    }
}
