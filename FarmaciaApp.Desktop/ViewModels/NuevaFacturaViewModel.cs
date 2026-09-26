/**
 * @file NuevaFacturaViewModel.cs
 * @brief Lógica del registro de una venta.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Producto agregado a la venta, con su cantidad.
     */
    public class LineaFactura
    {
        /** Producto con su precio del día. */
        public ProductoVenta Producto { get; init; }
        /** Unidades. */
        public int Cantidad { get; init; }
        /** Nombre del producto. */
        public string Nombre => Producto.ProNombre;
        /** Precio por unidad con descuento. */
        public decimal PrecioUnitario => Producto.PrecioFinal;
        /** Precio por cantidad. */
        public decimal Subtotal => PrecioUnitario * Cantidad;
        /** Descuento aplicado como texto, o vacío. */
        public string Promocion => Producto.Descuento > 0 ? $"-{Producto.Descuento:0.#}%" : "";
    }

    /**
     * @brief Arma la venta, calcula los totales y la registra.
     */
    public partial class NuevaFacturaViewModel : ObservableObject
    {
        private readonly FacturaService _service;
        private readonly Window _window;

        /** Clientes que se pueden facturar. */
        public List<Seleccion> Clientes { get; }
        /** Vendedores. */
        public List<Seleccion> Vendedores { get; }
        /** Productos con su precio del día. */
        public List<ProductoVenta> Productos { get; }
        /** Metodos de pago. */
        public string[] MetodosPago => FacturaService.MetodosPago;

        /** Productos agregados. */
        public ObservableCollection<LineaFactura> Lineas { get; } = new();

        /** Indica si se puede elegir el vendedor; un empleado siempre vende a su nombre. */
        public bool PuedeElegirVendedor => Sesion.EsAdmin || !Sesion.Activa;

        /** Cliente de la venta. */
        [ObservableProperty]
        private Seleccion clienteSeleccionado;

        /** Vendedor de la venta. */
        [ObservableProperty]
        private Seleccion vendedorSeleccionado;

        /** Método de pago. */
        [ObservableProperty]
        private string metodoPago = FacturaService.MetodosPago[0];

        /** Producto que se va a agregar. */
        [ObservableProperty]
        private ProductoVenta productoSeleccionado;

        /** Cantidad que se va a agregar. */
        [ObservableProperty]
        private decimal? cantidad = 1;

        /** Línea seleccionada en la tabla. */
        [ObservableProperty]
        private LineaFactura lineaSeleccionada;

        /** Total sin IVA. */
        [ObservableProperty]
        private decimal subtotal;

        /** IVA incluido en el total. */
        [ObservableProperty]
        private decimal iva;

        /** Total a pagar. */
        [ObservableProperty]
        private decimal total;

        /** Error de validación o de guardado. */
        [ObservableProperty]
        private string errorMessage;

        /** Agrega el producto seleccionado. */
        public IRelayCommand AgregarLineaCommand { get; }
        /** Quita la línea seleccionada. */
        public IRelayCommand QuitarLineaCommand { get; }
        /** Registra la venta. */
        public IAsyncRelayCommand GuardarCommand { get; }
        /** Cierra sin guardar. */
        public IRelayCommand CancelarCommand { get; }

        /**
         * @brief Carga clientes, vendedores y productos y fija el vendedor del empleado.
         * @param window Ventana del formulario
         */
        public NuevaFacturaViewModel(Window window)
        {
            _window = window;
            _service = new FacturaService();

            Clientes = _service.ObtenerClientes().ToList();
            Vendedores = _service.ObtenerVendedores().ToList();
            Productos = _service.ObtenerProductosParaVenta().ToList();
            if (PuedeElegirVendedor)
            {
                VendedorSeleccionado = Vendedores.Count == 1 ? Vendedores[0] : null;
            }
            else
            {
                VendedorSeleccionado = Vendedores.FirstOrDefault(v => v.Id == Sesion.VenId);
                if (VendedorSeleccionado == null)
                    ErrorMessage = "Tu usuario no está asociado a un vendedor. Pide al administrador que lo configure.";
            }

            AgregarLineaCommand = new RelayCommand(AgregarLinea);
            QuitarLineaCommand = new RelayCommand(QuitarLinea, () => LineaSeleccionada != null);
            GuardarCommand = new AsyncRelayCommand(Guardar, () => Lineas.Count > 0);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        /**
         * @brief Habilita el botón Quitar línea.
         * @param value Línea seleccionada
         */
        partial void OnLineaSeleccionadaChanged(LineaFactura value) => QuitarLineaCommand.NotifyCanExecuteChanged();

        /**
         * @brief Agrega el producto con su cantidad, o la suma a su línea si ya estaba.
         *
         * No permite superar el stock disponible.
         */
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

        /**
         * @brief Quita la línea seleccionada.
         */
        private void QuitarLinea()
        {
            if (LineaSeleccionada == null) return;
            Lineas.Remove(LineaSeleccionada);
            ActualizarTotales();
        }

        /**
         * @brief Recalcula el total y separa el IVA.
         */
        private void ActualizarTotales()
        {
            Total = Lineas.Sum(l => l.Subtotal);
            (Subtotal, Iva) = Factura.DesglosarIva(Total);
            GuardarCommand.NotifyCanExecuteChanged();
        }

        /**
         * @brief Registra la venta y muestra el número de factura.
         */
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
