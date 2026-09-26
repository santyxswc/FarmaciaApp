/**
 * @file FacturasViewModel.cs
 * @brief Lógica del listado de facturas.
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
     * @brief Lista y busca facturas, muestra su detalle y abre el registro de ventas.
     */
    public partial class FacturasViewModel : ObservableObject
    {
        private readonly FacturaService _service;

        /** Facturas que se muestran. */
        [ObservableProperty]
        private ObservableCollection<Factura> facturas;

        /** Factura seleccionada. */
        [ObservableProperty]
        private Factura seleccionado;

        /** Texto de búsqueda. */
        [ObservableProperty]
        private string searchTerm;

        /** Vuelve a cargar la lista. */
        public IRelayCommand RefreshCommand { get; }
        /** Busca con el texto escrito. */
        public IRelayCommand BuscarCommand { get; }
        /** Abre el detalle de la factura seleccionada. */
        public IAsyncRelayCommand VerDetalleCommand { get; }
        /** Abre el registro de una venta. */
        public IAsyncRelayCommand NuevaFacturaCommand { get; }

        /**
         * @brief Crea los comandos y carga las facturas.
         */
        public FacturasViewModel()
        {
            _service = new FacturaService();
            RefreshCommand = new RelayCommand(CargarFacturas);
            BuscarCommand = new RelayCommand(Buscar);
            VerDetalleCommand = new AsyncRelayCommand(AbrirDetalle, () => Seleccionado != null);
            NuevaFacturaCommand = new AsyncRelayCommand(AbrirNuevaFactura);
            CargarFacturas();
        }

        /**
         * @brief Habilita Ver detalle según la selección.
         * @param value Factura seleccionada
         */
        partial void OnSeleccionadoChanged(Factura value) => VerDetalleCommand.NotifyCanExecuteChanged();

        /**
         * @brief Carga todas las facturas.
         */
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

        /**
         * @brief Busca facturas; si el texto esta vacío, muestra todas.
         */
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

        /**
         * @brief Abre la ventana de detalle con las líneas de la factura.
         */
        private async Task AbrirDetalle()
        {
            if (Seleccionado == null) return;

            try
            {
                var factura = _service.ObtenerDetalle(Seleccionado.FacNumFactura);
                await Dialogs.ShowForm(new FacturaDetalleView { DataContext = factura });
            }
            catch (Exception ex)
            {
                await Dialogs.Error("Error al cargar la factura: " + ex.Message);
            }
        }

        /**
         * @brief Abre el registro de una venta y recarga la lista al guardar.
         */
        private async Task AbrirNuevaFactura()
        {
            NuevaFacturaView window;
            try
            {
                window = new NuevaFacturaView();
                window.DataContext = new NuevaFacturaViewModel(window);
            }
            catch (Exception ex)
            {
                await Dialogs.Error("Error al preparar la factura: " + ex.Message);
                return;
            }

            if (await Dialogs.ShowForm(window)) CargarFacturas();
        }
    }
}
