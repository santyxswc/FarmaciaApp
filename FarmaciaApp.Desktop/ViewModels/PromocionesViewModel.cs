/**
 * @file PromocionesViewModel.cs
 * @brief Lógica del listado de promociones.
 * @author Santiago Caicedo
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Lista, busca y abre los formularios de promociones.
     */
    public partial class PromocionesViewModel : ObservableObject
    {
        private readonly PromocionService _service;

        /** Promociones que se muestran. */
        [ObservableProperty]
        private ObservableCollection<Promocion> promociones;

        /** Registro seleccionado en la tabla. */
        [ObservableProperty]
        private Promocion seleccionado;

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
        /** Muestra solo las promociones vigentes. */
        public IRelayCommand VerActivasCommand { get; }

        /**
         * @brief Crea los comandos y carga la lista.
         */
        public PromocionesViewModel()
        {
            _service = new PromocionService();

            AgregarCommand = new AsyncRelayCommand(AbrirAgregar);
            EditarCommand = new AsyncRelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(Cargar);
            BuscarCommand = new RelayCommand(Buscar);
            VerActivasCommand = new RelayCommand(() => Ejecutar(() => Promociones = new ObservableCollection<Promocion>(_service.ObtenerPromocionesActivas())));

            Cargar();
        }

        /**
         * @brief Habilita Editar y Eliminar según la selección.
         * @param value Registro seleccionado
         */
        partial void OnSeleccionadoChanged(Promocion value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        /**
         * @brief Carga todos los registros.
         */
        private void Cargar() => Ejecutar(() => Promociones = new ObservableCollection<Promocion>(_service.ObtenerPromociones()));

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
            var window = new AgregarEditarPromocionView();
            window.DataContext = new AgregarEditarPromocionViewModel(window);
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        /**
         * @brief Abre el formulario con el registro seleccionado y recarga la lista al guardar.
         */
        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarPromocionView();
            var vm = new AgregarEditarPromocionViewModel(window);
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

            if (!await Dialogs.Confirm($"¿Eliminar la promoción '{Seleccionado.PrmDescripcion}'?")) return;

            try
            {
                if (!_service.EliminarPromocion(Seleccionado.PrmId))
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

            Ejecutar(() => Promociones = new ObservableCollection<Promocion>(_service.Buscar(SearchTerm)));
        }
    }
}
