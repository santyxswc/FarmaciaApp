using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.ComponentModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class AgregarEditarClienteViewModel : ObservableObject
    {
        private readonly ClienteService _service;
        private readonly Window _ownerWindow;
        private Cliente _form;

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
                }
            }
        }

        [ObservableProperty]
        private string errorMessage;

        public RelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public AgregarEditarClienteViewModel(Window owner)
        {
            _service = new ClienteService();
            _ownerWindow = owner;

            SaveCommand = new RelayCommand(Save, CanExecuteSave);
            CancelCommand = new RelayCommand(() => _ownerWindow.Close(false));

            Form = new Cliente();
        }

        private void Form_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
            SaveCommand.NotifyCanExecuteChanged();

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

        private bool CanExecuteSave()
        {
            return Form != null &&
                   !string.IsNullOrWhiteSpace(Form.PerNombre) &&
                   !string.IsNullOrWhiteSpace(Form.PerApellido);
        }

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
