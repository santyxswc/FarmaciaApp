using CommunityToolkit.Mvvm.ComponentModel;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;

using CommunityToolkit.Mvvm.Input;


namespace FarmaciaApp.UI.ViewModels
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

        public IRelayCommand AgregarCommand { get; }
        public IRelayCommand EditarCommand { get; }
        public IRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ProductosViewModel()
        {
            _service = new ProductoService();

            AgregarCommand = new RelayCommand(AbrirAgregar);
            EditarCommand = new RelayCommand(AbrirEditar);
            EliminarCommand = new RelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarProductos);
            BuscarCommand = new RelayCommand(Buscar);

            CargarProductos();
        }

        private void CargarProductos()
        {
            Productos = new ObservableCollection<Producto>(_service.ObtenerProductos());
        }

        private void AbrirAgregar()
        {
            var window = new AgregarEditarProductoView();
            var vm = new AgregarEditarProductoViewModel(window);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarProductos();
        }

        private void AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarProductoView();
            var vm = new AgregarEditarProductoViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarProductos();
        }

        private void Eliminar()
        {
            if (Seleccionado == null) return;

            var confirm = MessageBox.Show($"¿Eliminar {Seleccionado.ProNombre}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                bool ok = _service.EliminarProducto(Seleccionado.ProId);
                if (!ok)
                {
                    MessageBox.Show("No se pudo eliminar el producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    CargarProductos();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                CargarProductos();
                return;
            }

            Productos = new ObservableCollection<Producto>(_service.Buscar(SearchTerm));
        }
    }
}

