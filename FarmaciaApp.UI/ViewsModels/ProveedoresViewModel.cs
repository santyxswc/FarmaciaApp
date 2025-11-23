using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class ProveedoresViewModel : ObservableObject
    {
        private readonly ProveedorService _service;

        [ObservableProperty]
        private ObservableCollection<Proveedor> proveedores;

        [ObservableProperty]
        private Proveedor seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IRelayCommand AgregarCommand { get; }
        public IRelayCommand EditarCommand { get; }
        public IRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public ProveedoresViewModel()
        {
            _service = new ProveedorService();

            AgregarCommand = new RelayCommand(AbrirAgregar);
            EditarCommand = new RelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new RelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarProveedores);
            BuscarCommand = new RelayCommand(Buscar);

            CargarProveedores();
        }

        private void CargarProveedores()
        {
            Proveedores = new ObservableCollection<Proveedor>(_service.ObtenerProveedores());
        }

        private void AbrirAgregar()
        {
            var window = new AgregarEditarProveedorView();
            var vm = new AgregarEditarProveedorViewModel(window);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarProveedores();
        }

        private void AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarProveedorView();
            var vm = new AgregarEditarProveedorViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarProveedores();
        }

        private void Eliminar()
        {
            if (Seleccionado == null) return;

            var confirm = MessageBox.Show(
                $"¿Eliminar al proveedor '{Seleccionado.ProNombre}'?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                bool ok = _service.EliminarProveedor(Seleccionado.ProId);
                if (!ok)
                {
                    MessageBox.Show("No se pudo eliminar el proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    CargarProveedores();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                CargarProveedores();
                return;
            }

            Proveedores = new ObservableCollection<Proveedor>(_service.Buscar(SearchTerm));
        }
    }
}