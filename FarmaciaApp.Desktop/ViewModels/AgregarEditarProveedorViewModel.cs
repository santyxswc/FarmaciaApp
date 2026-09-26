using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class AgregarEditarProveedorViewModel : ObservableObject
    {
        private readonly ProveedorService _service;
        private readonly Window _window;
        private int _proveedorId;

        [ObservableProperty]
        private string titulo = "Agregar Proveedor";

        [ObservableProperty]
        private string nombre;

        [ObservableProperty]
        private string contacto;

        [ObservableProperty]
        private string telefono;

        public IAsyncRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public AgregarEditarProveedorViewModel(Window window)
        {
            _window = window;
            _service = new ProveedorService();

            GuardarCommand = new AsyncRelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        public void LoadFromModel(Proveedor p)
        {
            _proveedorId = p.ProId;
            Titulo = "Editar Proveedor";
            Nombre = p.ProNombre;
            Contacto = p.ProContacto;
            Telefono = p.ProTelefono;
        }

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
