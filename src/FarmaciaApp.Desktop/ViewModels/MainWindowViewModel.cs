/**
 * @file MainWindowViewModel.cs
 * @brief Lógica de la ventana principal.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;

using FarmaciaApp.Core.Sesion;
namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Navegación entre secciones, datos del turno y cierre de sesión.
     *
     * Tambien es el DataContext de la vista de inicio, para que sus tarjetas naveguen.
     */
    public partial class MainWindowViewModel : ObservableObject
    {
        /** Reglas de negocio de usuarios. */
        private readonly UsuarioService _usuarios;
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;
        /** Creación de ventanas y secciones. */
        private readonly FabricaVistas _vistas;
        /** Ventana del formulario. */
        private readonly Window _window;

        /** Vista que se muestra en el area de contenido. */
        [ObservableProperty]
        private Control currentView;

        /** Sección abierta, para resaltarla en el menu lateral. */
        [ObservableProperty]
        private string seccionActual;

        /** Indica si se muestra la sección de administración. */
        public bool EsAdmin => _sesion.EsAdmin;
        /** Nombre del usuario del turno. */
        public string NombreUsuario => _sesion.Usuario?.Nombre ?? "";
        /** Rol del usuario del turno. */
        public string Rol => _sesion.Usuario?.Rol ?? "";
        /** Título de la ventana con el usuario del turno. */
        public string TituloVentana => $"Farmacia · {NombreUsuario}";
        /** Rol y hora de inicio del turno. */
        public string DescripcionTurno => $"{Rol} · turno desde {_sesion.InicioTurno:HH:mm}";

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
         * @param usuarios Reglas de negocio de usuarios
         * @param sesion Usuario del turno
         * @param vistas Creación de ventanas y secciones
         */
        public MainWindowViewModel(Window window, UsuarioService usuarios, ISesionUsuario sesion, FabricaVistas vistas)
        {
            _usuarios = usuarios;
            _sesion = sesion;
            _vistas = vistas;
            _window = window;

            GoHomeCommand = new RelayCommand(() => Navegar("Inicio", () => new HomeView { DataContext = this }));
            GoProductosCommand = new RelayCommand(() => Navegar("Productos", () => _vistas.Seccion<ProductosView, ProductosViewModel>()));
            GoClientesCommand = new RelayCommand(() => Navegar("Clientes", () => _vistas.Seccion<ClientesView, ClientesViewModel>()));
            GoFacturasCommand = new RelayCommand(() => Navegar("Facturas", () => _vistas.Seccion<FacturasView, FacturasViewModel>()));
            GoReclamosCommand = new RelayCommand(() => Navegar("Reclamos", () => _vistas.Seccion<ReclamosView, ReclamosViewModel>()));
            GoPersonasCommand = new RelayCommand(() => Navegar("Personas", () => _vistas.Seccion<PersonasView, PersonasViewModel>()));
            GoProveedoresCommand = new RelayCommand(() => Navegar("Proveedores", () => _vistas.Seccion<ProveedoresView, ProveedoresViewModel>()));
            GoPromocionesCommand = new RelayCommand(() => Navegar("Promociones", () => _vistas.Seccion<PromocionesView, PromocionesViewModel>()));
            GoReportesCommand = new RelayCommand(() => Navegar("Reportes", () => _vistas.Seccion<ReportesView, ReportesViewModel>()), () => EsAdmin);
            GoMovimientosCommand = new RelayCommand(() => Navegar("Movimientos", () => _vistas.Seccion<MovimientosView, MovimientosViewModel>()), () => EsAdmin);
            GoUsuariosCommand = new RelayCommand(() => Navegar("Usuarios", () => _vistas.Seccion<UsuariosView, UsuariosViewModel>()), () => EsAdmin);
            CerrarSesionCommand = new AsyncRelayCommand(CerrarSesion);
            CambiarClaveCommand = new AsyncRelayCommand(CambiarClave);

            GoHomeCommand.Execute(null);
        }

        /**
         * @brief Cierra el turno cuando se cierra la ventana principal sin usar "Cerrar sesión".
         */
        public void CerrarTurnoPorVentana()
        {
            if (_sesion.Activa)
                _usuarios.CerrarSesion("ventana cerrada");
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

            _usuarios.CerrarSesion();

            var (login, _) = _vistas.Ventana<LoginView, LoginViewModel>();
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
            var (ventana, _) = _vistas.Ventana<CambiarClaveView, CambiarClaveViewModel>();
            if (await Dialogs.ShowForm(ventana))
                await Dialogs.Info("Tu contraseña se cambió correctamente.", "Contraseña actualizada");
        }
    }
}
