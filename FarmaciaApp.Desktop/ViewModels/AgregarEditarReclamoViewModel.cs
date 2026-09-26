/**
 * @file AgregarEditarReclamoViewModel.cs
 * @brief Lógica del formulario de reclamo.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Crea un reclamo sobre una factura o edita su descripción y estado.
     */
    public partial class AgregarEditarReclamoViewModel : ObservableObject
    {
        private readonly ReclamoService _service;
        private readonly Window _window;
        private decimal _reclamoId;

        /** Facturas que se pueden reclamar. */
        public List<Seleccion> Facturas { get; }
        /** Estados posibles. */
        public string[] Estados => ReclamoService.Estados;

        /** Título de la ventana. */
        [ObservableProperty]
        private string titulo = "Nuevo reclamo";

        /** Indica si se crea un reclamo; al editar no se puede cambiar la factura. */
        [ObservableProperty]
        private bool esNuevo = true;

        /** Factura reclamada. */
        [ObservableProperty]
        private Seleccion facturaSeleccionada;

        /** Descripción del problema. */
        [ObservableProperty]
        private string descripcion;

        /** Estado del reclamo. */
        [ObservableProperty]
        private string estado = ReclamoService.Estados[0];

        /** Error de validación o de guardado. */
        [ObservableProperty]
        private string errorMessage;

        /** Comando Guardar. */
        public IRelayCommand GuardarCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelarCommand { get; }

        /**
         * @brief Crea el formulario y carga las facturas.
         * @param window Ventana del formulario
         */
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

        /**
         * @brief Carga un reclamo existente para editarlo.
         * @param r Reclamo a editar
         */
        public void LoadFromModel(Reclamo r)
        {
            _reclamoId = r.RecId;
            Titulo = $"Editar reclamo N° {r.RecId}";
            EsNuevo = false;
            FacturaSeleccionada = Facturas.FirstOrDefault(f => f.Id == r.FacNumFactura);
            Descripcion = r.RecDescripcion;
            Estado = Estados.Contains(r.RecEstado) ? r.RecEstado : Estados[0];
        }

        /**
         * @brief Guarda el reclamo y cierra el formulario; si falla, muestra el error.
         */
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
