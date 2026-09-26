using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class ReportesViewModel : ObservableObject
    {
        private readonly ReporteService _service = new();

        [ObservableProperty]
        private DateTime? desde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [ObservableProperty]
        private DateTime? hasta = DateTime.Today;

        [ObservableProperty]
        private ResumenVentas resumen = new();

        [ObservableProperty]
        private string periodo;

        [ObservableProperty]
        private string errorMessage;

        public ObservableCollection<VentaPorVendedor> VentasPorVendedor { get; } = new();
        public ObservableCollection<ProductoVendido> ProductosMasVendidos { get; } = new();

        public IRelayCommand ConsultarCommand { get; }
        public IRelayCommand HoyCommand { get; }
        public IRelayCommand SemanaCommand { get; }
        public IRelayCommand MesCommand { get; }

        public ReportesViewModel()
        {
            ConsultarCommand = new RelayCommand(Consultar);
            HoyCommand = new RelayCommand(() => Rango(DateTime.Today, DateTime.Today));
            SemanaCommand = new RelayCommand(() => Rango(DateTime.Today.AddDays(-6), DateTime.Today));
            MesCommand = new RelayCommand(() => Rango(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today));
            Consultar();
        }

        private void Rango(DateTime inicio, DateTime fin)
        {
            Desde = inicio;
            Hasta = fin;
            Consultar();
        }

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
