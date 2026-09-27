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

using FarmaciaApp.Core.Sesion;
namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Crea o edita una persona y, si es administrador, su rol de vendedor.
     */
    public partial class AgregarEditarPersonaViewModel : ObservableObject
    {
        /** Usuario del turno. */
        private readonly ISesionUsuario _sesion;
        /** Reglas de negocio de personas. */
        private readonly PersonaService _service;
        /** Ventana del formulario. */
        private readonly Window _window;

        /** Indica si se muestran las acciones de administrador. */
        public bool EsAdmin => _sesion.EsAdmin;
        /** Id de la persona que se edita; 0 si es nueva. */
        private int _personaId;
        /** Indica si la persona ya era vendedora antes de editarla. */
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
         * @param service Reglas de negocio de personas
         * @param sesion Usuario del turno (permisos)
         */
        public AgregarEditarPersonaViewModel(Window window, PersonaService service, ISesionUsuario sesion)
        {
            _sesion = sesion;
            _service = service;
            _window = window;

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
