/**
 * @file AgregarEditarProductoViewModel.cs
 * @brief Lógica del formulario de producto.
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
     * @brief Crea o edita un producto y válida el formulario mientras se escribe.
     */
    public partial class AgregarEditarProductoViewModel : ObservableObject
    {
        private readonly ProductoService _service;
        private readonly Window _ownerWindow;
        private Producto _form;

        /**
         * @brief Producto que se edita en el formulario.
         *
         * Al cambiar se vuelve a evaluar si se puede guardar.
         */
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
                    OnPropertyChanged(nameof(Titulo));
                }
            }
        }

        /** Título de la ventana según si se crea o se edita. */
        public string Titulo => Form?.ProId > 0 ? "Editar producto" : "Nuevo producto";

        /** Error de validación o de guardado. */
        [ObservableProperty]
        private string errorMessage;

        /** Comando Guardar; se habilita cuando el formulario es válido. */
        public RelayCommand SaveCommand { get; }
        /** Comando Cancelar. */
        public IRelayCommand CancelCommand { get; }

        /** Indica si se está editando un producto existente. */
        public bool IsEditMode => Form?.ProId > 0;

        /**
         * @brief Crea el formulario vacío.
         * @param owner Ventana del formulario
         */
        public AgregarEditarProductoViewModel(Window owner)
        {
            _service = new ProductoService();
            _ownerWindow = owner;
            SaveCommand = new RelayCommand(Save, CanExecuteSave);
            CancelCommand = new RelayCommand(() => _ownerWindow.Close(false));
            Form = new Producto();
        }

        /**
         * @brief Reevalúa el botón Guardar cuando cambia un campo.
         * @param sender Modelo
         * @param e Campo que cambio
         */
        private void Form_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
            SaveCommand.NotifyCanExecuteChanged();

        /**
         * @brief Carga un producto existente para editarlo.
         * @param p Producto a editar
         */
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

        /**
         * @brief Indica si los campos obligatorios son válidos.
         * @return true si se puede guardar
         */
        private bool CanExecuteSave()
        {
            if (Form == null) return false;

            return !string.IsNullOrWhiteSpace(Form.ProNombre) &&
                   Form.ProPrecio > 0 &&
                   Form.ProStock >= 0;
        }

        /**
         * @brief Guarda y cierra el formulario; si falla, muestra el error.
         */
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
