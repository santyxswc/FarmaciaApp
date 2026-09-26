using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class AgregarEditarPromocionViewModel : ObservableObject
    {
        private readonly PromocionService _service;
        private readonly Window _window;
        private int _promocionId;

        [ObservableProperty]
        private string titulo = "Nueva promoción";

        [ObservableProperty]
        private string descripcion;

        [ObservableProperty]
        private decimal descuento;

        // DatePicker de Avalonia trabaja con DateTimeOffset?
        [ObservableProperty]
        private DateTimeOffset? fechaInicio = DateTimeOffset.Now;

        [ObservableProperty]
        private DateTimeOffset? fechaFin = DateTimeOffset.Now.AddDays(30);

        public IAsyncRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public AgregarEditarPromocionViewModel(Window window)
        {
            _window = window;
            _service = new PromocionService();

            GuardarCommand = new AsyncRelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        public void LoadFromModel(Promocion p)
        {
            _promocionId = p.PrmId;
            Titulo = "Editar promoción";
            Descripcion = p.PrmDescripcion;
            Descuento = p.PrmDescuento;
            FechaInicio = p.PrmFechaIni;
            FechaFin = p.PrmFechaFin;
        }

        private async Task Guardar()
        {
            try
            {
                var promocion = new Promocion
                {
                    PrmId = _promocionId,
                    PrmDescripcion = Descripcion,
                    PrmDescuento = Descuento,
                    PrmFechaIni = (FechaInicio ?? DateTimeOffset.Now).DateTime,
                    PrmFechaFin = (FechaFin ?? DateTimeOffset.Now).DateTime
                };

                if (_promocionId == 0)
                {
                    _service.CrearPromocion(promocion);
                    await Dialogs.Info("Promoción creada exitosamente", "Éxito");
                }
                else
                {
                    _service.ActualizarPromocion(promocion);
                    await Dialogs.Info("Promoción actualizada exitosamente", "Éxito");
                }

                _window.Close(true);
            }
            catch (Exception ex)
            {
                await Dialogs.Error($"Error: {ex.Message}");
            }
        }
    }
}
