using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
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
        public IAsyncRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ReclamosViewModel()
        {
            _service = new ReclamoService();

            AgregarCommand = new RelayCommand(() => { });
            EditarCommand = new RelayCommand(() => { }, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarReclamos);
            BuscarCommand = new RelayCommand(Buscar);

            CargarReclamos();
        }

        partial void OnSeleccionadoChanged(Reclamo value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        private void CargarReclamos()
        {
            try
            {
                Reclamos = new ObservableCollection<Reclamo>(_service.ObtenerReclamos());
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error($"Error al cargar reclamos: {ex.Message}", "Error de Base de Datos");
            }
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                CargarReclamos();
                return;
            }

            try
            {
                Reclamos = new ObservableCollection<Reclamo>(_service.Buscar(SearchTerm));
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error("Error: " + ex.Message);
            }
        }

        private async Task Eliminar()
        {
            if (Seleccionado == null) return;
            if (!await Dialogs.Confirm($"¿Eliminar el reclamo N° {Seleccionado.RecId}?", "Confirmar eliminación")) return;

            try
            {
                if (_service.EliminarReclamo(Seleccionado.RecId))
                    CargarReclamos();
            }
            catch (Exception ex)
            {
                await Dialogs.Error($"Error al eliminar: {ex.Message}");
            }
        }
    }
}
