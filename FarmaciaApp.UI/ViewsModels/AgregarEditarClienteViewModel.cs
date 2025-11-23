using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Windows;
using System;

namespace FarmaciaApp.UI.ViewModels
{
    // Debe ser parcial
    public partial class AgregarEditarClienteViewModel : ObservableObject
    {
        private readonly ClienteService _service;
        private readonly Window _ownerWindow;

        // Propiedad Form enlazada al modelo observable Cliente
        [ObservableProperty]
        private Cliente form;

        [ObservableProperty]
        private string errorMessage;

        // Comandos que controlan la ventana
        public RelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public AgregarEditarClienteViewModel(Window owner)
        {
            _service = new ClienteService();
            _ownerWindow = owner;

            // Inicialización de comandos y formulario
            SaveCommand = new RelayCommand(Save, CanExecuteSave);
            CancelCommand = new RelayCommand(Close);

            Form = new Cliente(); // Inicia un nuevo cliente
        }

        // -----------------------------------------------------------------
        // MÉTODO REQUERIDO: Carga el cliente existente para edición
        // -----------------------------------------------------------------
        public void LoadFromModel(Cliente c)
        {
            if (c == null) return;

            // Creamos una copia para evitar modificar el objeto original
            Form = new Cliente
            {
                PerId = c.PerId,
                PerNombre = c.PerNombre,
                PerApellido = c.PerApellido,
                // ... (copiar el resto de propiedades) ...
            };
        }

        private bool CanExecuteSave()
        {
            // Lógica de validación (Ej: El nombre y apellido no pueden estar vacíos)
            return Form != null &&
                   !string.IsNullOrWhiteSpace(Form.PerNombre) &&
                   !string.IsNullOrWhiteSpace(Form.PerApellido);
        }

        private void Save()
        {
            try
            {
                // Lógica de GUARDAR (usando _service.CrearCliente o _service.ActualizarCliente)
                // ...

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