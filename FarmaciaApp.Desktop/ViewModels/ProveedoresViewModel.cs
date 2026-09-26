using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class ProveedoresViewModel : ObservableObject
    {
        private readonly ProveedorService _service;

        [ObservableProperty]
        private ObservableCollection<Proveedor> proveedores;

        [ObservableProperty]
        private Proveedor seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IAsyncRelayCommand AgregarCommand { get; }
        public IAsyncRelayCommand EditarCommand { get; }
        public IAsyncRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ProveedoresViewModel()
        {
            _service = new ProveedorService();

            AgregarCommand = new AsyncRelayCommand(AbrirAgregar);
            EditarCommand = new AsyncRelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(Cargar);
            BuscarCommand = new RelayCommand(Buscar);

            Cargar();
        }

        partial void OnSeleccionadoChanged(Proveedor value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        private void Cargar() => Ejecutar(() => Proveedores = new ObservableCollection<Proveedor>(_service.ObtenerProveedores()));

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
            var window = new AgregarEditarProveedorView();
            window.DataContext = new AgregarEditarProveedorViewModel(window);
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarProveedorView();
            var vm = new AgregarEditarProveedorViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            if (await Dialogs.ShowForm(window)) Cargar();
        }

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
