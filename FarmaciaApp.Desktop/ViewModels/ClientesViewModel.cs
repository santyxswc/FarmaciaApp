using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
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

        public IAsyncRelayCommand AgregarCommand { get; }
        public IAsyncRelayCommand EditarCommand { get; }
        public IAsyncRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ClientesViewModel()
        {
            _service = new ClienteService();

            AgregarCommand = new AsyncRelayCommand(AbrirAgregar);
            EditarCommand = new AsyncRelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarClientes);
            BuscarCommand = new RelayCommand(Buscar);

            CargarClientes();
        }

        partial void OnSeleccionadoChanged(Cliente value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        private void CargarClientes()
        {
            try
            {
                Clientes = new ObservableCollection<Cliente>(_service.ObtenerClientes());
                Seleccionado = null;
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error($"Error al cargar clientes: {ex.Message}", "Error de Base de Datos");
            }
        }

        private async Task AbrirAgregar()
        {
            var view = new AgregarEditarClienteView();
            view.DataContext = new AgregarEditarClienteViewModel(view);
            if (await Dialogs.ShowForm(view)) CargarClientes();
        }

        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var view = new AgregarEditarClienteView();
            var viewModel = new AgregarEditarClienteViewModel(view);
            viewModel.LoadFromModel(Seleccionado);
            view.DataContext = viewModel;
            if (await Dialogs.ShowForm(view)) CargarClientes();
        }

        private async Task Eliminar()
        {
            if (Seleccionado == null) return;

            if (!await Dialogs.Confirm($"¿Eliminar a {Seleccionado.PerNombre} {Seleccionado.PerApellido}?")) return;

            try
            {
                if (_service.EliminarCliente(Seleccionado.PerId))
                    CargarClientes();
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
                CargarClientes();
                return;
            }

            try
            {
                Clientes = new ObservableCollection<Cliente>(_service.Buscar(SearchTerm));
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error("Error: " + ex.Message);
            }
        }
    }
}
