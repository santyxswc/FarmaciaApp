using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class AgregarEditarReclamoViewModel : ObservableObject
    {
        private readonly ReclamoService _service;
        private readonly Window _window;
        private decimal _reclamoId;

        public List<Seleccion> Facturas { get; }
        public string[] Estados => ReclamoService.Estados;

        [ObservableProperty]
        private string titulo = "Nuevo Reclamo";

        [ObservableProperty]
        private bool esNuevo = true;

        [ObservableProperty]
        private Seleccion facturaSeleccionada;

        [ObservableProperty]
        private string descripcion;

        [ObservableProperty]
        private string estado = ReclamoService.Estados[0];

        [ObservableProperty]
        private string errorMessage;

        public IRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public AgregarEditarReclamoViewModel(Window window)
        {
            _window = window;
            _service = new ReclamoService();

            Facturas = new FacturaService().ObtenerFacturas()
                .Select(f => new Seleccion { Id = f.FacNumFactura, Nombre = $"N° {f.FacNumFactura} · {f.ClienteNombre} · {f.FacFecha:dd/MM/yyyy}" })
                .ToList();

            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        public void LoadFromModel(Reclamo r)
        {
            _reclamoId = r.RecId;
            Titulo = $"Editar Reclamo N° {r.RecId}";
            EsNuevo = false;
            FacturaSeleccionada = Facturas.FirstOrDefault(f => f.Id == r.FacNumFactura);
            Descripcion = r.RecDescripcion;
            Estado = Estados.Contains(r.RecEstado) ? r.RecEstado : Estados[0];
        }

        private void Guardar()
        {
            try
            {
                var reclamo = new Reclamo
                {
                    RecId = _reclamoId,
                    FacNumFactura = FacturaSeleccionada?.Id ?? 0,
                    RecDescripcion = Descripcion,
                    RecEstado = Estado
                };

                if (EsNuevo)
                    _service.CrearReclamo(reclamo);
                else
                    _service.ActualizarReclamo(reclamo);

                _window.Close(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
