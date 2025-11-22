using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class FacturasViewModel : ObservableObject
    {
        private readonly FacturaService _service; // Asumimos que ya creaste FacturaService

        [ObservableProperty]
        private ObservableCollection<Factura> facturas;

        [ObservableProperty]
        private Factura seleccionado;

        [ObservableProperty]
        private string searchTerm;

        // Comandos de la UI
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }
        public IRelayCommand VerDetalleCommand { get; } // Añadimos un comando para ver los detalles

        public FacturasViewModel()
        {
            _service = new FacturaService();

            // Inicialización de Comandos
            RefreshCommand = new RelayCommand(CargarFacturas);
            BuscarCommand = new RelayCommand(Buscar);
            VerDetalleCommand = new RelayCommand(AbrirDetalle, () => Seleccionado != null);

            // Carga inicial
            CargarFacturas();
        }

        private void CargarFacturas()
        {
            try
            {
                // Llama al servicio del CORE para obtener la lista
                Facturas = new ObservableCollection<Factura>(_service.ObtenerFacturas());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar facturas: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                CargarFacturas();
                return;
            }
            // Asumimos que FacturaService tiene un método Buscar
            Facturas = new ObservableCollection<Factura>(_service.Buscar(SearchTerm));
        }

        private void AbrirDetalle()
        {
            if (Seleccionado == null) return;
            // Aquí se abriría una nueva ventana/modal para mostrar los ítems de la factura.
            MessageBox.Show($"Abriendo detalles de Factura N° {Seleccionado.FacNumFactura}. Vendedor: {Seleccionado.VendedorNombre}", "Detalle de Factura");
        }
    }
}