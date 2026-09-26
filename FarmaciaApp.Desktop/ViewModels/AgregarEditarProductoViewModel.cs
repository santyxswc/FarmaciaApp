using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.ComponentModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class AgregarEditarProductoViewModel : ObservableObject
    {
        private readonly ProductoService _service;
        private readonly Window _ownerWindow;
        private Producto _form;

        public Producto Form
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

        public bool IsEditMode => Form?.ProId > 0;

        public AgregarEditarProductoViewModel(Window owner)
        {
            _service = new ProductoService();
            _ownerWindow = owner;
            SaveCommand = new RelayCommand(Save, CanExecuteSave);
            CancelCommand = new RelayCommand(() => _ownerWindow.Close(false));
            Form = new Producto();
        }

        private void Form_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
            SaveCommand.NotifyCanExecuteChanged();

        public void LoadFromModel(Producto p)
        {
            if (p == null) return;
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
                    _service.ActualizarProducto(Form);
                else
                    Form.ProId = _service.CrearProducto(Form);

                _ownerWindow.Close(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
