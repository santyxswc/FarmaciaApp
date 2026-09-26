using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class PromocionesViewModel : ObservableObject
    {
        private readonly PromocionService _service;

        [ObservableProperty]
        private ObservableCollection<Promocion> promociones;

        [ObservableProperty]
        private Promocion seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IAsyncRelayCommand AgregarCommand { get; }
        public IAsyncRelayCommand EditarCommand { get; }
        public IAsyncRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }
        public IRelayCommand VerActivasCommand { get; }

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

        partial void OnSeleccionadoChanged(Promocion value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        private void Cargar() => Ejecutar(() => Promociones = new ObservableCollection<Promocion>(_service.ObtenerPromociones()));

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

        private async Task AbrirAgregar()
        {
            var window = new AgregarEditarPromocionView();
            window.DataContext = new AgregarEditarPromocionViewModel(window);
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarPromocionView();
            var vm = new AgregarEditarPromocionViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            if (await Dialogs.ShowForm(window)) Cargar();
        }

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
