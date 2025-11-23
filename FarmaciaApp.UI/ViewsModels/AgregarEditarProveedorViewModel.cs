using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class AgregarEditarProveedorViewModel : ObservableObject
    {
        private readonly ProveedorService _service;
        private readonly Window _window;
        private int _proveedorId;

        [ObservableProperty]
        private string titulo = "Agregar Proveedor";

        [ObservableProperty]
        private string nombre;

        [ObservableProperty]
        private string contacto;

        [ObservableProperty]
        private string telefono;

        public IRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public AgregarEditarProveedorViewModel(Window window)
        {
            _window = window;
            _service = new ProveedorService();

            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(Cancelar);
        }

        public void LoadFromModel(Proveedor p)
        {
            _proveedorId = p.ProId;
            Titulo = "Editar Proveedor";
            Nombre = p.ProNombre;
            Contacto = p.ProContacto;
            Telefono = p.ProTelefono;
        }

        private void Guardar()
        {
            try
            {
                var proveedor = new Proveedor
                {
                    ProId = _proveedorId,
                    ProNombre = Nombre,
                    ProContacto = Contacto,
                    ProTelefono = Telefono
                };

                if (_proveedorId == 0)
                {
                    _service.CrearProveedor(proveedor);
                    MessageBox.Show("Proveedor creado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _service.ActualizarProveedor(proveedor);
                    MessageBox.Show("Proveedor actualizado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _window.DialogResult = true;
                _window.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancelar()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}