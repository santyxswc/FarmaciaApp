using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class AgregarEditarPersonaViewModel : ObservableObject
    {
        private readonly PersonaService _service;
        private readonly Window _window;
        private int _personaId;
        private bool _eraVendedor;

        [ObservableProperty]
        private string titulo = "Agregar Persona";

        [ObservableProperty]
        private string nombre;

        [ObservableProperty]
        private string apellido;

        [ObservableProperty]
        private string direccion;

        [ObservableProperty]
        private string telefono;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private bool esVendedor;

        public IAsyncRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public AgregarEditarPersonaViewModel(Window window)
        {
            _window = window;
            _service = new PersonaService();

            GuardarCommand = new AsyncRelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        public void LoadFromModel(Persona p)
        {
            _personaId = p.PerId;
            Titulo = "Editar Persona";
            Nombre = p.PerNombre;
            Apellido = p.PerApellido;
            Direccion = p.PerDireccion;
            Telefono = p.PerTelefono;
            Email = p.PerEmail;
            EsVendedor = _eraVendedor = p.EsVendedor;
        }

        private async Task Guardar()
        {
            try
            {
                var persona = new Persona
                {
                    PerId = _personaId,
                    PerNombre = Nombre,
                    PerApellido = Apellido,
                    PerDireccion = Direccion,
                    PerTelefono = Telefono,
                    PerEmail = Email
                };

                if (_personaId == 0)
                {
                    _personaId = _service.CrearPersona(persona);
                    if (EsVendedor)
                        _service.AsignarVendedor(_personaId, true);
                    await Dialogs.Info("Persona creada exitosamente", "Éxito");
                }
                else
                {
                    _service.ActualizarPersona(persona);
                    if (EsVendedor != _eraVendedor)
                    {
                        _service.AsignarVendedor(_personaId, EsVendedor);
                        _eraVendedor = EsVendedor;
                    }
                    await Dialogs.Info("Persona actualizada exitosamente", "Éxito");
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
