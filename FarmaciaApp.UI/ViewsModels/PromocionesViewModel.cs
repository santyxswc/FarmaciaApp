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
    public partial class PromocionesViewModel : ObservableObject
    {
        private readonly PromocionService _service;

        [ObservableProperty]
        private ObservableCollection<Promocion> promociones;

        [ObservableProperty]
        private Promocion seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IRelayCommand AgregarCommand { get; }
        public IRelayCommand EditarCommand { get; }
        public IRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }
        public IRelayCommand VerActivasCommand { get; }

        public PromocionesViewModel()
        {
            _service = new PromocionService();

            AgregarCommand = new RelayCommand(AbrirAgregar);
            EditarCommand = new RelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new RelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarPromociones);
            BuscarCommand = new RelayCommand(Buscar);
            VerActivasCommand = new RelayCommand(MostrarActivas);

            CargarPromociones();
        }

        private void CargarPromociones()
        {
            Promociones = new ObservableCollection<Promocion>(_service.ObtenerPromociones());
        }

        private void MostrarActivas()
        {
            Promociones = new ObservableCollection<Promocion>(_service.ObtenerPromocionesActivas());
        }

        private void AbrirAgregar()
        {
            var window = new AgregarEditarPromocionView();
            var vm = new AgregarEditarPromocionViewModel(window);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarPromociones();
        }

        private void AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarPromocionView();
            var vm = new AgregarEditarPromocionViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarPromociones();
        }

        private void Eliminar()
        {
            if (Seleccionado == null) return;

            var confirm = MessageBox.Show(
                $"¿Eliminar la promoción '{Seleccionado.PrmDescripcion}'?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                bool ok = _service.EliminarPromocion(Seleccionado.PrmId);
                if (!ok)
                {
                    MessageBox.Show("No se pudo eliminar la promoción.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    CargarPromociones();
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
                CargarPromociones();
                return;
            }

            Promociones = new ObservableCollection<Promocion>(_service.Buscar(SearchTerm));
        }
    }
}