using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public class LineaFactura
    {
        public ProductoVenta Producto { get; init; }
        public int Cantidad { get; init; }
        public string Nombre => Producto.ProNombre;
        public decimal PrecioUnitario => Producto.PrecioFinal;
        public decimal Subtotal => PrecioUnitario * Cantidad;
        public string Promocion => Producto.Descuento > 0 ? $"-{Producto.Descuento:0.#}%" : "";
    }

    public partial class NuevaFacturaViewModel : ObservableObject
    {
        private readonly FacturaService _service;
        private readonly Window _window;

        public List<Seleccion> Clientes { get; }
        public List<Seleccion> Vendedores { get; }
        public List<ProductoVenta> Productos { get; }
        public string[] MetodosPago => FacturaService.MetodosPago;

        public ObservableCollection<LineaFactura> Lineas { get; } = new();

        [ObservableProperty]
        private Seleccion clienteSeleccionado;

        [ObservableProperty]
        private Seleccion vendedorSeleccionado;

        [ObservableProperty]
        private string metodoPago = FacturaService.MetodosPago[0];

        [ObservableProperty]
        private ProductoVenta productoSeleccionado;

        [ObservableProperty]
        private decimal? cantidad = 1;

        [ObservableProperty]
        private LineaFactura lineaSeleccionada;

        [ObservableProperty]
        private decimal subtotal;

        [ObservableProperty]
        private decimal iva;

        [ObservableProperty]
        private decimal total;

        [ObservableProperty]
        private string errorMessage;

        public IRelayCommand AgregarLineaCommand { get; }
        public IRelayCommand QuitarLineaCommand { get; }
        public IAsyncRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public NuevaFacturaViewModel(Window window)
        {
            _window = window;
            _service = new FacturaService();

            Clientes = _service.ObtenerClientes().ToList();
            Vendedores = _service.ObtenerVendedores().ToList();
            Productos = _service.ObtenerProductosParaVenta().ToList();
            VendedorSeleccionado = Vendedores.Count == 1 ? Vendedores[0] : null;

            AgregarLineaCommand = new RelayCommand(AgregarLinea);
            QuitarLineaCommand = new RelayCommand(QuitarLinea, () => LineaSeleccionada != null);
            GuardarCommand = new AsyncRelayCommand(Guardar, () => Lineas.Count > 0);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        partial void OnLineaSeleccionadaChanged(LineaFactura value) => QuitarLineaCommand.NotifyCanExecuteChanged();

        private void AgregarLinea()
        {
            ErrorMessage = null;

            if (ProductoSeleccionado == null)
            {
                ErrorMessage = "Selecciona un producto.";
                return;
            }

            int cantidadNueva = (int)(Cantidad ?? 0);
            if (cantidadNueva <= 0)
            {
                ErrorMessage = "La cantidad debe ser mayor a cero.";
                return;
            }

            // Si el producto ya esta en la factura se suma a su linea
            var existente = Lineas.FirstOrDefault(l => l.Producto.ProId == ProductoSeleccionado.ProId);
            int cantidadTotal = cantidadNueva + (existente?.Cantidad ?? 0);
            if (cantidadTotal > ProductoSeleccionado.Stock)
            {
                ErrorMessage = $"Solo hay {ProductoSeleccionado.Stock} unidades de {ProductoSeleccionado.ProNombre}.";
                return;
            }

            var linea = new LineaFactura { Producto = ProductoSeleccionado, Cantidad = cantidadTotal };
            if (existente != null)
                Lineas[Lineas.IndexOf(existente)] = linea;
            else
                Lineas.Add(linea);

            Cantidad = 1;
            ActualizarTotales();
        }

        private void QuitarLinea()
        {
            if (LineaSeleccionada == null) return;
            Lineas.Remove(LineaSeleccionada);
            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            Total = Lineas.Sum(l => l.Subtotal);
            (Subtotal, Iva) = Factura.DesglosarIva(Total);
            GuardarCommand.NotifyCanExecuteChanged();
        }

        private async Task Guardar()
        {
            ErrorMessage = null;
            try
            {
                decimal numero = _service.CrearFactura(
                    ClienteSeleccionado?.Id ?? 0,
                    VendedorSeleccionado?.Id ?? 0,
                    MetodoPago,
                    Lineas.Select(l => new FacturaProductoDetalle { ProId = l.Producto.ProId, Cantidad = l.Cantidad }));

                await Dialogs.Info($"Factura N° {numero} registrada por {Total:C0}.", "Venta registrada");
                _window.Close(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
