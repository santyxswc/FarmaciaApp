using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Windows;
using System;
using System.ComponentModel; // <--- AGREGADO
using System.Linq;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class AgregarEditarProductoViewModel : ObservableObject
    {
        private readonly ProductoService _service;
        private readonly Window _ownerWindow;

        // Backing field y propiedad manual
        private Producto _form;
        public Producto Form
        {
            get => _form;
            set
            {
                // 1. Desuscribirse del objeto anterior
                if (_form != null)
                {
                    _form.PropertyChanged -= Form_PropertyChanged;
                }

                // 2. Establecer la nueva propiedad y notificar cambio al ViewModel
                if (SetProperty(ref _form, value))
                {
                    // 3. Suscribirse al nuevo objeto para monitorear cambios internos
                    if (_form != null)
                    {
                        _form.PropertyChanged += Form_PropertyChanged;
                    }
                    // ESTA LÍNEA REQUERÍA QUE SaveCommand EXISTIERA
                    // En el constructor, esto sucede solo DESPUÉS de inicializar SaveCommand
                    if (SaveCommand != null)
                    {
                        SaveCommand.NotifyCanExecuteChanged();
                    }
                }
            }
        }

        [ObservableProperty]
        private string errorMessage;

        // Comandos
        public RelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public bool IsEditMode => Form?.ProId > 0;

        public AgregarEditarProductoViewModel(Window owner)
        {
            _service = new ProductoService();
            _ownerWindow = owner;

            // ----------------------------------------------------
            // CORRECCIÓN CLAVE: Inicializar comandos PRIMERO
            // ----------------------------------------------------
            SaveCommand = new RelayCommand(Save, CanExecuteSave);
            CancelCommand = new RelayCommand(Close);

            // ----------------------------------------------------
            // SEGUNDO: Inicializar Form. Ahora SaveCommand NO es null.
            // ----------------------------------------------------
            Form = new Producto();
        }

        private void Form_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveCommand.NotifyCanExecuteChanged();
        }

        public void LoadFromModel(Producto p)
        {
            if (p == null) return;

            // Esto llama al setter de 'Form', que ya está corregido.
            Form = new Producto
            {
                ProId = p.ProId,
                ProNombre = p.ProNombre,
                ProPrecio = p.ProPrecio,
                ProStock = p.ProStock,
                ProDescripcion = p.ProDescripcion
            };
        }

        private bool CanExecuteSave()
        {
            if (Form == null) return false;

            return !string.IsNullOrWhiteSpace(Form.ProNombre) &&
                   Form.ProPrecio > 0 &&
                   Form.ProStock >= 0;
        }

        private void Save()
        {
            if (!CanExecuteSave())
            {
                ErrorMessage = "Corrige los errores del formulario.";
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    _service.ActualizarProducto(Form);
                }
                else
                {
                    int newId = _service.CrearProducto(Form);
                    Form.ProId = newId;
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