/**
 * @file ProveedoresViewModel.cs
 * @brief Lógica del listado de proveedores.
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
     * @brief Lista, busca y abre los formularios de proveedores.
     */
    public partial class ProveedoresViewModel : ObservableObject
    {
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;
        /** Creación de ventanas y secciones. */
        private readonly FabricaVistas _vistas;
        /** Reglas de negocio de proveedores. */
        private readonly ProveedorService _service;

        /** Indica si se muestran las acciones de administrador. */
        public bool EsAdmin => _sesion.EsAdmin;

        /** Proveedores que se muestran. */
        [ObservableProperty]
        private ObservableCollection<Proveedor> proveedores;

        /** Registro seleccionado en la tabla. */
        [ObservableProperty]
        private Proveedor seleccionado;

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
         * @param service Reglas de negocio de proveedors
         * @param vistas Creación de ventanas y secciones
         * @param sesion Usuario del turno (permisos)
         */
        public ProveedoresViewModel(ProveedorService service, FabricaVistas vistas, ISesionUsuario sesion)
        {
            _sesion = sesion;
            _service = service;
            _vistas = vistas;

            AgregarCommand = new AsyncRelayCommand(AbrirAgregar);
            EditarCommand = new AsyncRelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(Cargar);
            BuscarCommand = new RelayCommand(Buscar);

            Cargar();
        }

        /**
         * @brief Habilita Editar y Eliminar según la selección.
         * @param value Registro seleccionado
         */
        partial void OnSeleccionadoChanged(Proveedor value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        /**
         * @brief Carga todos los registros.
         */
        private void Cargar() => Ejecutar(() => Proveedores = new ObservableCollection<Proveedor>(_service.ObtenerProveedores()));

        /**
         * @brief Ejecuta una consulta y muestra el error si falla.
         * @param accion Consulta a ejecutar
         */
        private void Ejecutar(Action accion)
        {
            try
            {
                accion();
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error("Error: " + ex.Message, "Error de Base de Datos");
            }
        }

        /**
         * @brief Abre el formulario para crear y recarga la lista al guardar.
         */
        private async Task AbrirAgregar()
        {
            var (window, _) = _vistas.Ventana<AgregarEditarProveedorView, AgregarEditarProveedorViewModel>();
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        /**
         * @brief Abre el formulario con el registro seleccionado y recarga la lista al guardar.
         */
        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var (window, vm) = _vistas.Ventana<AgregarEditarProveedorView, AgregarEditarProveedorViewModel>();
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        /**
         * @brief Pide confirmación y elimina el registro seleccionado.
         */
        private async Task Eliminar()
        {
            if (Seleccionado == null) return;

            if (!await Dialogs.Confirm($"¿Eliminar al proveedor '{Seleccionado.ProNombre}'?")) return;

            try
            {
                if (!_service.EliminarProveedor(Seleccionado.ProId))
                    await Dialogs.Error("No se pudo eliminar el registro.");
                else
                    Cargar();
            }
            catch (Exception ex)
            {
                await Dialogs.Error("Error: " + ex.Message);
            }
        }

        /**
         * @brief Busca con el texto escrito; si está vacío, muestra todos.
         */
        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Cargar();
                return;
            }

            Ejecutar(() => Proveedores = new ObservableCollection<Proveedor>(_service.Buscar(SearchTerm)));
        }
    }
}
