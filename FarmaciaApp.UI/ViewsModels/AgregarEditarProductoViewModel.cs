using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Windows;

using FarmaciaApp.UI.Models;
using System;
using System.Linq;


namespace FarmaciaApp.UI.ViewModels
{
    public partial class AgregarEditarProductoViewModel : ObservableObject
    {
        private readonly ProductoService _service;
        private readonly Window _ownerWindow;

        [ObservableProperty] private ProductoFormModel form;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private bool canSave;

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public bool IsEditMode => Form?.ProId > 0;

        public AgregarEditarProductoViewModel(Window owner)
        {
            _service = new ProductoService();
            _ownerWindow = owner;

            Form = new ProductoFormModel();
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Close);

            // Valida inicialmente
            UpdateCanSave();
            // Suscribir cambios simples: cada set de propiedad del Form invocará UpdateCanSave
        }

        public void LoadFromModel(Producto p)
        {
            if (p == null) return;
            Form = new ProductoFormModel
            {
                ProId = p.ProId,
                ProNombre = p.ProNombre,
                ProPrecio = p.ProPrecio,
                ProStock = p.ProStock,
                ProDescripcion = p.ProDescripcion
            };

            UpdateCanSave();
        }

        private void UpdateCanSave()
        {
            CanSave = string.IsNullOrEmpty(Form[nameof(Form.ProNombre)]) &&
                      string.IsNullOrEmpty(Form[nameof(Form.ProPrecio)]) &&
                      string.IsNullOrEmpty(Form[nameof(Form.ProStock)]);
        }

        private void Save()
        {
            try
            {
                // Forzar validación antes de guardar
                UpdateCanSave();
                if (!CanSave)
                {
                    ErrorMessage = "Corrige los errores del formulario.";
                    return;
                }

                var model = new Producto
                {
                    ProId = Form.ProId,
                    ProNombre = Form.ProNombre,
                    ProPrecio = Form.ProPrecio,
                    ProStock = Form.ProStock,
                    ProDescripcion = Form.ProDescripcion
                };

                if (IsEditMode)
                {
                    _service.ActualizarProducto(model);
                }
                else
                {
                    int newId = _service.CrearProducto(model);
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
