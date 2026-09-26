using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly Window _window;

        [ObservableProperty]
        private Control currentView;

        // Seccion abierta, para resaltarla en el menu lateral
        [ObservableProperty]
        private string seccionActual;

        public bool EsAdmin => Sesion.EsAdmin;
        public string NombreUsuario => Sesion.Usuario?.Nombre ?? "";
        public string Rol => Sesion.Usuario?.Rol ?? "";
        public string DescripcionTurno => $"{Rol} · turno desde {Sesion.InicioTurno:HH:mm}";

        public IRelayCommand GoHomeCommand { get; }
        public IRelayCommand GoProductosCommand { get; }
        public IRelayCommand GoClientesCommand { get; }
        public IRelayCommand GoFacturasCommand { get; }
        public IRelayCommand GoReclamosCommand { get; }
        public IRelayCommand GoPersonasCommand { get; }
        public IRelayCommand GoProveedoresCommand { get; }
        public IRelayCommand GoPromocionesCommand { get; }
        public IRelayCommand GoReportesCommand { get; }
        public IRelayCommand GoMovimientosCommand { get; }
        public IRelayCommand GoUsuariosCommand { get; }
        public IAsyncRelayCommand CerrarSesionCommand { get; }
        public IAsyncRelayCommand CambiarClaveCommand { get; }

        public MainWindowViewModel(Window window)
        {
            _window = window;

            GoHomeCommand = new RelayCommand(() => Navegar("Inicio", () => new HomeView { DataContext = this }));
            GoProductosCommand = new RelayCommand(() => Navegar("Productos", () => new ProductosView()));
            GoClientesCommand = new RelayCommand(() => Navegar("Clientes", () => new ClientesView()));
            GoFacturasCommand = new RelayCommand(() => Navegar("Facturas", () => new FacturasView()));
            GoReclamosCommand = new RelayCommand(() => Navegar("Reclamos", () => new ReclamosView()));
            GoPersonasCommand = new RelayCommand(() => Navegar("Personas", () => new PersonasView()));
            GoProveedoresCommand = new RelayCommand(() => Navegar("Proveedores", () => new ProveedoresView()));
            GoPromocionesCommand = new RelayCommand(() => Navegar("Promociones", () => new PromocionesView()));
            GoReportesCommand = new RelayCommand(() => Navegar("Reportes", () => new ReportesView()), () => EsAdmin);
            GoMovimientosCommand = new RelayCommand(() => Navegar("Movimientos", () => new MovimientosView()), () => EsAdmin);
            GoUsuariosCommand = new RelayCommand(() => Navegar("Usuarios", () => new UsuariosView()), () => EsAdmin);
            CerrarSesionCommand = new AsyncRelayCommand(CerrarSesion);
            CambiarClaveCommand = new AsyncRelayCommand(CambiarClave);

            GoHomeCommand.Execute(null);
        }

        private void Navegar(string seccion, Func<Control> crearVista)
        {
            CurrentView = crearVista();
            SeccionActual = seccion;
        }

        // Cambio de turno: se registra el cierre y se vuelve a la pantalla de inicio de sesion
        private async Task CerrarSesion()
        {
            if (!await Dialogs.Confirm($"¿Cerrar la sesión de {NombreUsuario}?\n\nLa aplicación volverá a la pantalla de inicio de sesión para el siguiente turno.", "Cerrar sesión"))
                return;

            new UsuarioService().CerrarSesion();

            var login = new LoginView();
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = login;
            login.Show();
            _window.Close();
        }

        private async Task CambiarClave()
        {
            var ventana = new CambiarClaveView();
            ventana.DataContext = new CambiarClaveViewModel(ventana);
            if (await Dialogs.ShowForm(ventana))
                await Dialogs.Info("Tu contraseña se cambió correctamente.", "Contraseña actualizada");
        }
    }
}
