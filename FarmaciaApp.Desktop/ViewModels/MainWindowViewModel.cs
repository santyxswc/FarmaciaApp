/**
 * @file MainWindowViewModel.cs
 * @brief Lógica de la ventana principal.
 * @author Santiago Caicedo
 */
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
    /**
     * @brief Navegación entre secciones, datos del turno y cierre de sesión.
     *
     * Tambien es el DataContext de la vista de inicio, para que sus tarjetas naveguen.
     */
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly Window _window;

        /** Vista que se muestra en el area de contenido. */
        [ObservableProperty]
        private Control currentView;

        /** Sección abierta, para resaltarla en el menu lateral. */
        [ObservableProperty]
        private string seccionActual;

        /** Indica si se muestra la sección de administración. */
        public bool EsAdmin => Sesion.EsAdmin;
        /** Nombre del usuario del turno. */
        public string NombreUsuario => Sesion.Usuario?.Nombre ?? "";
        /** Rol del usuario del turno. */
        public string Rol => Sesion.Usuario?.Rol ?? "";
        /** Rol y hora de inicio del turno. */
        public string DescripcionTurno => $"{Rol} · turno desde {Sesion.InicioTurno:HH:mm}";

        /** Abre la vista de inicio. */
        public IRelayCommand GoHomeCommand { get; }
        /** Abre productos. */
        public IRelayCommand GoProductosCommand { get; }
        /** Abre clientes. */
        public IRelayCommand GoClientesCommand { get; }
        /** Abre facturas. */
        public IRelayCommand GoFacturasCommand { get; }
        /** Abre reclamos. */
        public IRelayCommand GoReclamosCommand { get; }
        /** Abre personas. */
        public IRelayCommand GoPersonasCommand { get; }
        /** Abre proveedores. */
        public IRelayCommand GoProveedoresCommand { get; }
        /** Abre promociones. */
        public IRelayCommand GoPromocionesCommand { get; }
        /** Abre los reportes de ventas (solo administrador). */
        public IRelayCommand GoReportesCommand { get; }
        /** Abre los movimientos (solo administrador). */
        public IRelayCommand GoMovimientosCommand { get; }
        /** Abre los usuarios (solo administrador). */
        public IRelayCommand GoUsuariosCommand { get; }
        /** Cierra el turno y vuelve al inicio de sesión. */
        public IAsyncRelayCommand CerrarSesionCommand { get; }
        /** Abre el cambio de contraseña del usuario. */
        public IAsyncRelayCommand CambiarClaveCommand { get; }

        /**
         * @brief Crea los comandos de navegación y abre la vista de inicio.
         * @param window Ventana principal
         */
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

        /**
         * @brief Cambia la vista del area de contenido.
         * @param seccion Nombre de la sección para el menu
         * @param crearVista Función que crea la vista
         */
        private void Navegar(string seccion, Func<Control> crearVista)
        {
            CurrentView = crearVista();
            SeccionActual = seccion;
        }

        /**
         * @brief Cambio de turno: pide confirmación, registra el cierre y abre el inicio de sesión.
         */
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

        /**
         * @brief Abre el formulario para cambiar la contraseña propia.
         */
        private async Task CambiarClave()
        {
            var ventana = new CambiarClaveView();
            ventana.DataContext = new CambiarClaveViewModel(ventana);
            if (await Dialogs.ShowForm(ventana))
                await Dialogs.Info("Tu contraseña se cambió correctamente.", "Contraseña actualizada");
        }
    }
}
