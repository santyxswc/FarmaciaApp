/**
 * @file AgregarEditarPromocionViewModel.cs
 * @brief Lógica del formulario de promoción.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Crea o edita una promoción.
     */
    public partial class AgregarEditarPromocionViewModel : ObservableObject
    {
        private readonly PromocionService _service;
        private readonly Window _window;
        private int _promocionId;

        /** Título de la ventana. */
        [ObservableProperty]
        private string titulo = "Nueva promoción";

        /** Descripción. */
        [ObservableProperty]
        private string descripcion;

        /** Porcentaje de descuento. */
        [ObservableProperty]
        private decimal descuento;

        /** Fecha de inicio (el DatePicker usa DateTimeOffset). */
        [ObservableProperty]
        private DateTimeOffset? fechaInicio = DateTimeOffset.Now;

        /** Fecha de fin. */
        [ObservableProperty]
        private DateTimeOffset? fechaFin = DateTimeOffset.Now.AddDays(30);

        /** Comando Guardar. */
        public IAsyncRelayCommand GuardarCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelarCommand { get; }

        /**
         * @brief Crea el formulario con una vigencia de 30 días.
         * @param window Ventana del formulario
         */
        public AgregarEditarPromocionViewModel(Window window)
        {
            _window = window;
            _service = new PromocionService();

            GuardarCommand = new AsyncRelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        /**
         * @brief Carga una promoción existente para editarla.
         * @param p Promoción a editar
         */
        public void LoadFromModel(Promocion p)
        {
            _promocionId = p.PrmId;
            Titulo = "Editar promoción";
            Descripcion = p.PrmDescripcion;
            Descuento = p.PrmDescuento;
            FechaInicio = p.PrmFechaIni;
            FechaFin = p.PrmFechaFin;
        }

        /**
         * @brief Guarda la promoción y cierra el formulario.
         */
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
