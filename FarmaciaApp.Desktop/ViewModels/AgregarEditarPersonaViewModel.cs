/**
 * @file AgregarEditarPersonaViewModel.cs
 * @brief Lógica del formulario de persona.
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
     * @brief Crea o edita una persona y, si es administrador, su rol de vendedor.
     */
    public partial class AgregarEditarPersonaViewModel : ObservableObject
    {
        private readonly PersonaService _service;
        private readonly Window _window;
        private int _personaId;
        private bool _eraVendedor;

        /** Título de la ventana. */
        [ObservableProperty]
        private string titulo = "Nueva persona";

        /** Nombre. */
        [ObservableProperty]
        private string nombre;

        /** Apellido. */
        [ObservableProperty]
        private string apellido;

        /** Dirección. */
        [ObservableProperty]
        private string direccion;

        /** Teléfono. */
        [ObservableProperty]
        private string telefono;

        /** Correo electrónico. */
        [ObservableProperty]
        private string email;

        /** Indica si la persona es vendedor (solo lo cambia el administrador). */
        [ObservableProperty]
        private bool esVendedor;

        /** Comando Guardar. */
        public IAsyncRelayCommand GuardarCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelarCommand { get; }

        /**
         * @brief Crea el formulario vacío.
         * @param window Ventana del formulario
         */
        public AgregarEditarPersonaViewModel(Window window)
        {
            _window = window;
            _service = new PersonaService();

            GuardarCommand = new AsyncRelayCommand(Guardar);
            CancelarCommand = new RelayCommand(() => _window.Close(false));
        }

        /**
         * @brief Carga una persona existente para editarla.
         * @param p Persona a editar
         */
        public void LoadFromModel(Persona p)
        {
            _personaId = p.PerId;
            Titulo = "Editar persona";
            Nombre = p.PerNombre;
            Apellido = p.PerApellido;
            Direccion = p.PerDireccion;
            Telefono = p.PerTelefono;
            Email = p.PerEmail;
            EsVendedor = _eraVendedor = p.EsVendedor;
        }

        /**
         * @brief Guarda los datos y, si cambió, el rol de vendedor.
         */
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
