/**
 * @file AgregarEditarProveedorViewModel.cs
 * @brief Lógica del formulario de proveedor.
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
     * @brief Crea o edita un proveedor.
     */
    public partial class AgregarEditarProveedorViewModel : ObservableObject
    {
        private readonly ProveedorService _service;
        private readonly Window _window;
        private int _proveedorId;

        /** Título de la ventana. */
        [ObservableProperty]
        private string titulo = "Nuevo proveedor";

        /** Nombre de la empresa. */
        [ObservableProperty]
        private string nombre;

        /** Persona de contacto. */
        [ObservableProperty]
        private string contacto;

        /** Teléfono. */
        [ObservableProperty]
        private string telefono;

        /** Comando Guardar. */
        public IAsyncRelayCommand GuardarCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelarCommand { get; }

        /**
         * @brief Crea el formulario vacío.
         * @param window Ventana del formulario
         */
        public AgregarEditarProveedorViewModel(Window window)
        {
            _window = window;
            _service = new ProveedorService();

            GuardarCommand = new AsyncRelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        /**
         * @brief Carga un proveedor existente para editarlo.
         * @param p Proveedor a editar
         */
        public void LoadFromModel(Proveedor p)
        {
            _proveedorId = p.ProId;
            Titulo = "Editar proveedor";
            Nombre = p.ProNombre;
            Contacto = p.ProContacto;
            Telefono = p.ProTelefono;
        }

        /**
         * @brief Guarda el proveedor y cierra el formulario.
         */
        private async Task Guardar()
        {
            try
            {
                var proveedor = new Proveedor
                {
                    ProId = _proveedorId,
                    ProNombre = Nombre,
                    ProContacto = Contacto,
                    ProTelefono = Telefono
                };

                if (_proveedorId == 0)
                {
                    _service.CrearProveedor(proveedor);
                    await Dialogs.Info("Proveedor creado exitosamente", "Éxito");
                }
                else
                {
                    _service.ActualizarProveedor(proveedor);
                    await Dialogs.Info("Proveedor actualizado exitosamente", "Éxito");
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
