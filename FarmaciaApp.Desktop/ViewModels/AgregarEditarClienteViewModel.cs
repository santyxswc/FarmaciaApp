/**
 * @file AgregarEditarClienteViewModel.cs
 * @brief Lógica del formulario de cliente.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.ComponentModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Crea o edita un cliente y válida el formulario mientras se escribe.
     */
    public partial class AgregarEditarClienteViewModel : ObservableObject
    {
        private readonly ClienteService _service;
        private readonly Window _ownerWindow;
        private Cliente _form;

        /**
         * @brief Cliente que se edita en el formulario.
         *
         * Al cambiar se vuelve a evaluar si se puede guardar.
         */
        public Cliente Form
        {
            get => _form;
            set
            {
                if (_form != null) _form.PropertyChanged -= Form_PropertyChanged;
                if (SetProperty(ref _form, value))
                {
                    if (_form != null) _form.PropertyChanged += Form_PropertyChanged;
                    SaveCommand?.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(Titulo));
                }
            }
        }

        /** Título de la ventana según si se crea o se edita. */
        public string Titulo => Form?.PerId > 0 ? "Editar cliente" : "Nuevo cliente";

        /** Error de validación o de guardado. */
        [ObservableProperty]
        private string errorMessage;

        /** Comando Guardar; se habilita cuando el formulario es válido. */
        public RelayCommand SaveCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelCommand { get; }

        /**
         * @brief Crea el formulario vacío.
         * @param owner Ventana del formulario
         */
        public AgregarEditarClienteViewModel(Window owner)
        {
            _service = new ClienteService();
            _ownerWindow = owner;

            SaveCommand = new RelayCommand(Save, CanExecuteSave);
            CancelCommand = new RelayCommand(() => _ownerWindow.Close(false));

            Form = new Cliente();
        }

        /**
         * @brief Reevalúa el botón Guardar cuando cambia un campo.
         * @param sender Modelo
         * @param e Campo que cambio
         */
        private void Form_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
            SaveCommand.NotifyCanExecuteChanged();

        /**
         * @brief Carga un cliente existente para editarlo.
         * @param c Cliente a editar
         */
        public void LoadFromModel(Cliente c)
        {
            if (c == null) return;

            Form = new Cliente
            {
                PerId = c.PerId,
                PerNombre = c.PerNombre,
                PerApellido = c.PerApellido,
                PerDireccion = c.PerDireccion,
                PerTelefono = c.PerTelefono,
                PerEmail = c.PerEmail
            };
        }

        /**
         * @brief Indica si los campos obligatorios son válidos.
         * @return true si se puede guardar
         */
        private bool CanExecuteSave()
        {
            return Form != null &&
                   !string.IsNullOrWhiteSpace(Form.PerNombre) &&
                   !string.IsNullOrWhiteSpace(Form.PerApellido);
        }

        /**
         * @brief Guarda y cierra el formulario; si falla, muestra el error.
         */
        private void Save()
        {
            try
            {
                if (Form.PerId == 0)
                    _service.CrearCliente(Form);
                else
                    _service.ActualizarCliente(Form);

                _ownerWindow.Close(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
