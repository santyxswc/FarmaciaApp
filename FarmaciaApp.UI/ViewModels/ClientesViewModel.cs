using CommunityToolkit.Mvvm.ComponentModel;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class ClientesViewModel : ObservableObject
    {
        private readonly ClienteService _service;

        [ObservableProperty]
        private ObservableCollection<Cliente> clientes;

        [ObservableProperty]
        private Cliente seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public RelayCommand AgregarCommand { get; }
        public RelayCommand EditarCommand { get; }
        public RelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ClientesViewModel()
        {
            _service = new ClienteService();

            AgregarCommand = new RelayCommand(AbrirAgregar);
            EditarCommand = new RelayCommand(AbrirEditar, CanExecuteEdit);
            EliminarCommand = new RelayCommand(Eliminar, CanExecuteDelete);
            RefreshCommand = new RelayCommand(CargarClientes);
            BuscarCommand = new RelayCommand(Buscar);

            CargarClientes();
        }

        partial void OnSeleccionadoChanged(Cliente value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        private bool CanExecuteEdit() => Seleccionado != null;

        private bool CanExecuteDelete() => Seleccionado != null;

        private void CargarClientes()
        {
            Clientes = new ObservableCollection<Cliente>(_service.ObtenerClientes());
            Seleccionado = null;
        }

        private void AbrirAgregar()
        {
            var view = new AgregarEditarClienteView();
            var viewModel = new AgregarEditarClienteViewModel(view);
            view.DataContext = viewModel;

            if (view.ShowDialog() == true)
            {
                CargarClientes();
            }
        }

        private void AbrirEditar()
        {
            if (Seleccionado == null) return;

            var view = new AgregarEditarClienteView();
            var viewModel = new AgregarEditarClienteViewModel(view);
            viewModel.LoadFromModel(Seleccionado);
            view.DataContext = viewModel;

            if (view.ShowDialog() == true)
            {
                CargarClientes();
            }
        }

        private void Eliminar()
        {
            if (Seleccionado == null) return;

            var confirm = MessageBox.Show($"¿Eliminar a {Seleccionado.PerNombre} {Seleccionado.PerApellido}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (_service.EliminarCliente(Seleccionado.PerId))
                {
                    CargarClientes();
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
                CargarClientes();
                return;
            }

            Clientes = new ObservableCollection<Cliente>(_service.Buscar(SearchTerm));
        }
    }
}
