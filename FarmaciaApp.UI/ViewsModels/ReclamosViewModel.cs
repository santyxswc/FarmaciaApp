using CommunityToolkit.Mvvm.ComponentModel;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class ReclamosViewModel : ObservableObject
    {
        private readonly ReclamoService _service; // Asumimos que ya creaste ReclamoService

        [ObservableProperty]
        private ObservableCollection<Reclamo> reclamos;

        [ObservableProperty]
        private Reclamo seleccionado;

        [ObservableProperty]
        private string searchTerm;

        // Comandos de la UI (Similar a Clientes, para CRUD)
        public IRelayCommand AgregarCommand { get; }
        public IRelayCommand EditarCommand { get; }
        public IRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ReclamosViewModel()
        {
            _service = new ReclamoService();

            // Inicialización de Comandos
            AgregarCommand = new RelayCommand(AbrirAgregar);
            EditarCommand = new RelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new RelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarReclamos);
            BuscarCommand = new RelayCommand(Buscar);

            // Carga inicial
            CargarReclamos();
        }

        private void CargarReclamos()
        {
            try
            {
                // Llama al servicio del CORE para obtener la lista
                Reclamos = new ObservableCollection<Reclamo>(_service.ObtenerReclamos());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar reclamos: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                CargarReclamos();
                return;
            }
            Reclamos = new ObservableCollection<Reclamo>(_service.Buscar(SearchTerm));
        }

        // Simplemente un ejemplo de la lógica CRUD
        private void AbrirAgregar() { /* Lógica para abrir la vista de agregar */ }
        private void AbrirEditar() { /* Lógica para abrir la vista de editar */ }
        private void Eliminar()
        {
            if (Seleccionado == null) return;
            MessageBox.Show($"Eliminando Reclamo N° {Seleccionado.RecId}", "Confirmar");
            // ... _service.EliminarReclamo(Seleccionado.RecId) ...
        }
    }
}