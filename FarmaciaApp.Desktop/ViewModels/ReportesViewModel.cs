/**
 * @file ReportesViewModel.cs
 * @brief Lógica de los reportes de ventas.
 * @author Santiago Caicedo
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Consulta los reportes de ventas de un periodo.
     */
    public partial class ReportesViewModel : ObservableObject
    {
        private readonly ReporteService _service = new();

        /** Fecha inicial (por defecto, el primer día del mes). */
        [ObservableProperty]
        private DateTime? desde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        /** Fecha final. */
        [ObservableProperty]
        private DateTime? hasta = DateTime.Today;

        /** Totales del periodo. */
        [ObservableProperty]
        private ResumenVentas resumen = new();

        /** Periodo consultado como texto. */
        [ObservableProperty]
        private string periodo;

        /** Error de la consulta. */
        [ObservableProperty]
        private string errorMessage;

        /** Ventas de cada empleado. */
        public ObservableCollection<VentaPorVendedor> VentasPorVendedor { get; } = new();
        /** Productos más vendidos. */
        public ObservableCollection<ProductoVendido> ProductosMasVendidos { get; } = new();

        /** Consulta el periodo elegido. */
        public IRelayCommand ConsultarCommand { get; }
        /** Consulta el día de hoy. */
        public IRelayCommand HoyCommand { get; }
        /** Consulta los últimos 7 días. */
        public IRelayCommand SemanaCommand { get; }
        /** Consulta el mes actual. */
        public IRelayCommand MesCommand { get; }

        /**
         * @brief Crea los comandos y consulta el mes actual.
         */
        public ReportesViewModel()
        {
            ConsultarCommand = new RelayCommand(Consultar);
            HoyCommand = new RelayCommand(() => Rango(DateTime.Today, DateTime.Today));
            SemanaCommand = new RelayCommand(() => Rango(DateTime.Today.AddDays(-6), DateTime.Today));
            MesCommand = new RelayCommand(() => Rango(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today));
            Consultar();
        }

        /**
         * @brief Fija el periodo y consulta.
         * @param inicio Fecha inicial
         * @param fin Fecha final
         */
        private void Rango(DateTime inicio, DateTime fin)
        {
            Desde = inicio;
            Hasta = fin;
            Consultar();
        }

        /**
         * @brief Carga el resumen, las ventas por empleado y los productos más vendidos.
         */
        private void Consultar()
        {
            ErrorMessage = null;
            var inicio = Desde ?? DateTime.Today;
            var fin = Hasta ?? DateTime.Today;
            try
            {
                Resumen = _service.ObtenerResumen(inicio, fin);

                VentasPorVendedor.Clear();
                foreach (var v in _service.ObtenerVentasPorVendedor(inicio, fin)) VentasPorVendedor.Add(v);

                ProductosMasVendidos.Clear();
                foreach (var p in _service.ObtenerProductosMasVendidos(inicio, fin)) ProductosMasVendidos.Add(p);

                Periodo = inicio.Date == fin.Date ? $"{inicio:dd/MM/yyyy}" : $"Del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
