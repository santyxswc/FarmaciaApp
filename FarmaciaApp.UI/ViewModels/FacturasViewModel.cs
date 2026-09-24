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
        private readonly FacturaService _service;

        [ObservableProperty]
        private ObservableCollection<Factura> facturas;

        [ObservableProperty]
        private Factura seleccionado;

        [ObservableProperty]
        private string searchTerm;
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }
        public IRelayCommand VerDetalleCommand { get; }

        public FacturasViewModel()
        {
            _service = new FacturaService();
            RefreshCommand = new RelayCommand(CargarFacturas);
            BuscarCommand = new RelayCommand(Buscar);
            VerDetalleCommand = new RelayCommand(AbrirDetalle, () => Seleccionado != null);
            CargarFacturas();
        }

        private void CargarFacturas()
        {
            try
            {
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
            Facturas = new ObservableCollection<Factura>(_service.Buscar(SearchTerm));
        }

        private void AbrirDetalle()
        {
            if (Seleccionado == null) return;
            MessageBox.Show($"Abriendo detalles de Factura N° {Seleccionado.FacNumFactura}. Vendedor: {Seleccionado.VendedorNombre}", "Detalle de Factura");
        }
    }
}