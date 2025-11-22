using CommunityToolkit.Mvvm.ComponentModel;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
// Nota: Debes crear o usar una vista existente, por ahora usamos 'Views' genéricas.
using FarmaciaApp.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;

namespace FarmaciaApp.UI.ViewModels
{
    // Usa 'partial' para que las propiedades con [ObservableProperty] funcionen.
    public partial class ClientesViewModel : ObservableObject
    {
        private readonly ClienteService _service; // El servicio que crearemos en el Core

        // Propiedad que la DataGrid/ListView de la View usará como ItemsSource
        [ObservableProperty]
        private ObservableCollection<Cliente> clientes;

        // Propiedad que contiene el cliente seleccionado en la lista
        [ObservableProperty]
        private Cliente seleccionado;

        [ObservableProperty]
        private string searchTerm;

        // Definición de Comandos
        public IRelayCommand AgregarCommand { get; }
        public IRelayCommand EditarCommand { get; }
        public IRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ClientesViewModel()
        {
            // Instancia el servicio de datos
            _service = new ClienteService();

            // Inicialización de Comandos
            AgregarCommand = new RelayCommand(AbrirAgregar);
            // El botón 'Editar' y 'Eliminar' solo se habilitan si hay un cliente seleccionado
            EditarCommand = new RelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new RelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarClientes);
            BuscarCommand = new RelayCommand(Buscar);

            // Carga inicial al iniciar el ViewModel
            CargarClientes();
        }

        // Método de Carga de Datos
        private void CargarClientes()
        {
            // Llama al servicio del CORE para obtener la lista
            // Asegúrate de que 'ObtenerClientes()' en ClienteService.cs retorne datos.
            Clientes = new ObservableCollection<Cliente>(_service.ObtenerClientes());
        }

        // El resto de los métodos (AbrirAgregar, AbrirEditar, Eliminar, Buscar)
        // se implementan de forma similar a como lo hiciste en ProductosViewModel.cs

        private void AbrirAgregar()
        {
            // Reemplaza esto con tu View real para Agregar/Editar Cliente
            MessageBox.Show("Abriendo formulario para agregar cliente.", "Acción");
            // Ejemplo de recarga tras cerrar la ventana de edición/creación:
            // if (new AgregarEditarClienteView().ShowDialog() == true) CargarClientes();
        }

        private void AbrirEditar()
        {
            if (Seleccionado == null) return;
            MessageBox.Show($"Abriendo formulario para editar a {Seleccionado.PerNombre}.", "Acción");
        }

        private void Eliminar()
        {
            if (Seleccionado == null) return;

            var confirm = MessageBox.Show($"¿Eliminar a {Seleccionado.PerNombre} {Seleccionado.PerApellido}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                // bool ok = _service.EliminarCliente(Seleccionado.PerId);
                // if (!ok) { ... } else { CargarClientes(); }
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