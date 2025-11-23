using CommunityToolkit.Mvvm.ComponentModel;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.ComponentModel;
using FarmaciaApp.UI.Views;
using FarmaciaApp.UI.ViewModels;
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

        // Propiedad que contiene el cliente seleccionado en la lista
        [ObservableProperty]
        private Cliente seleccionado; // El cambio aquí dispara OnSeleccionadoChanged

        [ObservableProperty]
        private string searchTerm;

        // Definición de Comandos (deben ser RelayCommand si usas NotifyCanExecuteChanged)
        public RelayCommand AgregarCommand { get; } // Cambiado a RelayCommand
        public RelayCommand EditarCommand { get; }   // Cambiado a RelayCommand
        public RelayCommand EliminarCommand { get; } // Cambiado a RelayCommand
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ClientesViewModel()
        {
            _service = new ClienteService();

            // Inicialización de Comandos
            AgregarCommand = new RelayCommand(AbrirAgregar);

            // Usamos RelayCommand para tener acceso a NotifyCanExecuteChanged()
            EditarCommand = new RelayCommand(AbrirEditar, CanExecuteEdit);
            EliminarCommand = new RelayCommand(Eliminar, CanExecuteDelete);

            RefreshCommand = new RelayCommand(CargarClientes);
            BuscarCommand = new RelayCommand(Buscar);

            CargarClientes();
        }

        // -----------------------------------------------------------------
        // CORRECCIÓN CLAVE: Método parcial generado por [ObservableProperty]
        // -----------------------------------------------------------------
        partial void OnSeleccionadoChanged(Cliente value)
        {
            // Cuando la DataGrid establece la selección (o la quita a null), 
            // forzamos la reevaluación de los comandos de Editar y Eliminar.
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        // -----------------------------------------------------------------
        // PREDICADOS DE HABILITACIÓN/DESHABILITACIÓN
        // -----------------------------------------------------------------
        private bool CanExecuteEdit()
        {
            // La lógica se mantiene, pero ahora se llama a demanda
            return Seleccionado != null;
        }

        private bool CanExecuteDelete()
        {
            return Seleccionado != null;
        }

        // --- Métodos de Acción (Se mantienen) ---
        private void CargarClientes()
        {
            Clientes = new ObservableCollection<Cliente>(_service.ObtenerClientes());

            // OPCIONAL: Deseleccionar automáticamente para deshabilitar botones
            Seleccionado = null;
        }

        private void AbrirAgregar()
        {
            // 1. Crear la View (la ventana)
            var view = new AgregarEditarClienteView();

            // 2. Crear el ViewModel de edición, pasándole la View como 'owner'.
            // ¡IMPORTANTE! Asumimos que tienes un AgregarEditarClienteViewModel.cs
            var viewModel = new AgregarEditarClienteViewModel(view);

            // 3. Enlazar la View al ViewModel.
            view.DataContext = viewModel;

            // 4. Mostrar el diálogo. Si ShowDialog() devuelve true, el guardado fue exitoso.
            if (view.ShowDialog() == true)
            {
                CargarClientes(); // Recargar la lista para mostrar el nuevo cliente
            }
        }

        private void AbrirEditar()
        {
            if (Seleccionado == null) return;

            // 1. Crear la View
            var view = new AgregarEditarClienteView();

            // 2. Crear el ViewModel, pasándole la View.
            var viewModel = new AgregarEditarClienteViewModel(view);

            // 3. Cargar los datos del cliente seleccionado en el ViewModel de edición.
            // ¡IMPORTANTE! Este método 'LoadFromModel' debe existir en el VM de edición.
            viewModel.LoadFromModel(Seleccionado);

            // 4. Enlazar la View al ViewModel.
            view.DataContext = viewModel;

            // 5. Mostrar el diálogo.
            if (view.ShowDialog() == true)
            {
                CargarClientes(); // Recargar la lista para ver los cambios
            }
        }

        private void Eliminar()
        {
            if (Seleccionado == null) return;

            var confirm = MessageBox.Show($"¿Eliminar a {Seleccionado.PerNombre} {Seleccionado.PerApellido}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                // Aquí va la llamada al servicio _service.EliminarCliente(Seleccionado.PerId);
                // Si la eliminación es exitosa:
                // CargarClientes();
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