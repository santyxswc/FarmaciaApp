/**
 * @file ReclamosViewModel.cs
 * @brief Lógica del listado de reclamos.
 * @author Santiago Caicedo
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

using FarmaciaApp.Core.Sesion;
namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Lista, busca y abre los formularios de reclamos.
     */
    public partial class ReclamosViewModel : ObservableObject
    {
        private readonly ISesionUsuario _sesion;
        private readonly FabricaVistas _vistas;
        private readonly ReclamoService _service;

        /** Indica si se muestran las acciones de administrador. */
        public bool EsAdmin => _sesion.EsAdmin;

        /** Reclamos que se muestran. */
        [ObservableProperty]
        private ObservableCollection<Reclamo> reclamos;

        /** Registro seleccionado en la tabla. */
        [ObservableProperty]
        private Reclamo seleccionado;

        /** Texto de búsqueda. */
        [ObservableProperty]
        private string searchTerm;

        /** Abre el formulario para crear. */
        public IAsyncRelayCommand AgregarCommand { get; }
        /** Abre el formulario para editar el seleccionado. */
        public IAsyncRelayCommand EditarCommand { get; }
        /** Elimina el seleccionado después de confirmar. */
        public IAsyncRelayCommand EliminarCommand { get; }
        /** Vuelve a cargar la lista. */
        public IRelayCommand RefreshCommand { get; }
        /** Busca con el texto escrito. */
        public IRelayCommand BuscarCommand { get; }

        /**
         * @brief Crea los comandos y carga la lista.
         * @param service Reglas de negocio de reclamos
         * @param vistas Creación de ventanas y secciones
         * @param sesion Usuario del turno (permisos)
         */
        public ReclamosViewModel(ReclamoService service, FabricaVistas vistas, ISesionUsuario sesion)
        {
            _sesion = sesion;
            _service = service;
            _vistas = vistas;

            AgregarCommand = new AsyncRelayCommand(AbrirAgregar);
            EditarCommand = new AsyncRelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarReclamos);
            BuscarCommand = new RelayCommand(Buscar);

            CargarReclamos();
        }

        /**
         * @brief Habilita Editar y Eliminar según la selección.
         * @param value Reclamo seleccionado
         */
        partial void OnSeleccionadoChanged(Reclamo value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        /**
         * @brief Carga todos los reclamos.
         */
        private void CargarReclamos()
        {
            try
            {
                Reclamos = new ObservableCollection<Reclamo>(_service.ObtenerReclamos());
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error($"Error al cargar reclamos: {ex.Message}", "Error de Base de Datos");
            }
        }

        /**
         * @brief Busca reclamos; si el texto esta vacío, muestra todos.
         */
        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                CargarReclamos();
                return;
            }

            try
            {
                Reclamos = new ObservableCollection<Reclamo>(_service.Buscar(SearchTerm));
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error("Error: " + ex.Message);
            }
        }

        /**
         * @brief Abre el formulario para crear un reclamo.
         */
        private async Task AbrirAgregar()
        {
            var (window, _) = _vistas.Ventana<AgregarEditarReclamoView, AgregarEditarReclamoViewModel>();
            if (await Dialogs.ShowForm(window)) CargarReclamos();
        }

        /**
         * @brief Abre el formulario con el reclamo seleccionado.
         */
        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var (window, vm) = _vistas.Ventana<AgregarEditarReclamoView, AgregarEditarReclamoViewModel>();
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            if (await Dialogs.ShowForm(window)) CargarReclamos();
        }

        /**
         * @brief Pide confirmación y elimina el reclamo seleccionado.
         */
        private async Task Eliminar()
        {
            if (Seleccionado == null) return;
            if (!await Dialogs.Confirm($"¿Eliminar el reclamo N° {Seleccionado.RecId}?", "Confirmar eliminación")) return;

            try
            {
                if (_service.EliminarReclamo(Seleccionado.RecId))
                    CargarReclamos();
            }
            catch (Exception ex)
            {
                await Dialogs.Error($"Error al eliminar: {ex.Message}");
            }
        }
    }
}
