using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class ProductosViewModel : ObservableObject
    {
        private readonly ProductoService _service;

        [ObservableProperty]
        private ObservableCollection<Producto> productos;

        [ObservableProperty]
        private Producto seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IAsyncRelayCommand AgregarCommand { get; }
        public IAsyncRelayCommand EditarCommand { get; }
        public IAsyncRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ProductosViewModel()
        {
            _service = new ProductoService();

            AgregarCommand = new AsyncRelayCommand(AbrirAgregar);
            EditarCommand = new AsyncRelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(Cargar);
            BuscarCommand = new RelayCommand(Buscar);

            Cargar();
        }

        partial void OnSeleccionadoChanged(Producto value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        private void Cargar() => Ejecutar(() => Productos = new ObservableCollection<Producto>(_service.ObtenerProductos()));

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
            var window = new AgregarEditarProductoView();
            window.DataContext = new AgregarEditarProductoViewModel(window);
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarProductoView();
            var vm = new AgregarEditarProductoViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        private async Task Eliminar()
        {
            if (Seleccionado == null) return;

            if (!await Dialogs.Confirm($"¿Eliminar {Seleccionado.ProNombre}?")) return;

            try
            {
                if (!_service.EliminarProducto(Seleccionado.ProId))
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

            Ejecutar(() => Productos = new ObservableCollection<Producto>(_service.Buscar(SearchTerm)));
        }
    }
}
