/**
 * @file ClientesViewModel.cs
 * @brief Lógica del listado de clientes.
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
     * @brief Lista, busca y abre los formularios de clientes.
     */
    public partial class ClientesViewModel : ObservableObject
    {
        private readonly ClienteService _service;

        /** Clientes que se muestran. */
        [ObservableProperty]
        private ObservableCollection<Cliente> clientes;

        /** Registro seleccionado en la tabla. */
        [ObservableProperty]
        private Cliente seleccionado;

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
         */
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

        /**
         * @brief Habilita Editar y Eliminar según la selección.
         * @param value Cliente seleccionado
         */
        partial void OnSeleccionadoChanged(Cliente value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        /**
         * @brief Carga todos los clientes.
         */
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

        /**
         * @brief Abre el formulario para crear un cliente.
         */
        private async Task AbrirAgregar()
        {
            var view = new AgregarEditarClienteView();
            view.DataContext = new AgregarEditarClienteViewModel(view);
            if (await Dialogs.ShowForm(view)) CargarClientes();
        }

        /**
         * @brief Abre el formulario con el cliente seleccionado.
         */
        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var view = new AgregarEditarClienteView();
            var viewModel = new AgregarEditarClienteViewModel(view);
            viewModel.LoadFromModel(Seleccionado);
            view.DataContext = viewModel;
            if (await Dialogs.ShowForm(view)) CargarClientes();
        }

        /**
         * @brief Pide confirmación y elimina el cliente seleccionado.
         */
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

        /**
         * @brief Busca clientes; si el texto esta vacío, muestra todos.
         */
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
