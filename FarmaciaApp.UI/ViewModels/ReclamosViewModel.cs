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
        private readonly ReclamoService _service;

        [ObservableProperty]
        private ObservableCollection<Reclamo> reclamos;

        [ObservableProperty]
        private Reclamo seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IRelayCommand AgregarCommand { get; }
        public IRelayCommand EditarCommand { get; }
        public IRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ReclamosViewModel()
        {
            _service = new ReclamoService();

            AgregarCommand = new RelayCommand(AbrirAgregar);
            EditarCommand = new RelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new RelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarReclamos);
            BuscarCommand = new RelayCommand(Buscar);

            CargarReclamos();
        }

        private void CargarReclamos()
        {
            try
            {
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

        private void AbrirAgregar() { }
        private void AbrirEditar() { }

        private void Eliminar()
        {
            if (Seleccionado == null) return;
            var confirm = MessageBox.Show($"¿Eliminar el reclamo N° {Seleccionado.RecId}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (_service.EliminarReclamo(Seleccionado.RecId))
                {
                    CargarReclamos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
