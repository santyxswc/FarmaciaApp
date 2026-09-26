using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class FacturasViewModel : ObservableObject
    {
        private readonly FacturaService _service;

        [ObservableProperty]
        private ObservableCollection<Factura> facturas;

        [ObservableProperty]
        private Factura seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }
        public IAsyncRelayCommand VerDetalleCommand { get; }

        public FacturasViewModel()
        {
            _service = new FacturaService();
            RefreshCommand = new RelayCommand(CargarFacturas);
            BuscarCommand = new RelayCommand(Buscar);
            VerDetalleCommand = new AsyncRelayCommand(AbrirDetalle, () => Seleccionado != null);
            CargarFacturas();
        }

        partial void OnSeleccionadoChanged(Factura value) => VerDetalleCommand.NotifyCanExecuteChanged();

        private void CargarFacturas()
        {
            try
            {
                Facturas = new ObservableCollection<Factura>(_service.ObtenerFacturas());
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error($"Error al cargar facturas: {ex.Message}", "Error de Base de Datos");
            }
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                CargarFacturas();
                return;
            }

            try
            {
                Facturas = new ObservableCollection<Factura>(_service.Buscar(SearchTerm));
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error("Error: " + ex.Message);
            }
        }

        private async Task AbrirDetalle()
        {
            if (Seleccionado == null) return;
            await Dialogs.Info($"Abriendo detalles de Factura N° {Seleccionado.FacNumFactura}. Vendedor: {Seleccionado.VendedorNombre}", "Detalle de Factura");
        }
    }
}
