using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Windows;
using System;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class AgregarEditarClienteViewModel : ObservableObject
    {
        private readonly ClienteService _service;
        private readonly Window _ownerWindow;

        [ObservableProperty]
        private Cliente form;

        [ObservableProperty]
        private string errorMessage;

        public RelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public AgregarEditarClienteViewModel(Window owner)
        {
            _service = new ClienteService();
            _ownerWindow = owner;

            SaveCommand = new RelayCommand(Save, CanExecuteSave);
            CancelCommand = new RelayCommand(Close);

            Form = new Cliente();
        }

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
                {
                    _service.CrearCliente(Form);
                }
                else
                {
                    _service.ActualizarCliente(Form);
                }

                _ownerWindow.DialogResult = true;
                _ownerWindow.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void Close()
        {
            _ownerWindow.DialogResult = false;
            _ownerWindow.Close();
        }
    }
}
