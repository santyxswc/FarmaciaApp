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
    public partial class PersonasViewModel : ObservableObject
    {
        private readonly PersonaService _service;

        [ObservableProperty]
        private ObservableCollection<Persona> personas;

        [ObservableProperty]
        private Persona seleccionado;

        [ObservableProperty]
        private string searchTerm;

        public IRelayCommand AgregarCommand { get; }
        public IRelayCommand EditarCommand { get; }
        public IRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public PersonasViewModel()
        {
            _service = new PersonaService();

            AgregarCommand = new RelayCommand(AbrirAgregar);
            EditarCommand = new RelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new RelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(CargarPersonas);
            BuscarCommand = new RelayCommand(Buscar);

            CargarPersonas();
        }

        private void CargarPersonas()
        {
            Personas = new ObservableCollection<Persona>(_service.ObtenerPersonas());
        }

        private void AbrirAgregar()
        {
            var window = new AgregarEditarPersonaView();
            var vm = new AgregarEditarPersonaViewModel(window);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarPersonas();
        }

        private void AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarPersonaView();
            var vm = new AgregarEditarPersonaViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            window.Owner = Application.Current.MainWindow;
            var result = window.ShowDialog();
            if (result == true) CargarPersonas();
        }

        private void Eliminar()
        {
            if (Seleccionado == null) return;

            var confirm = MessageBox.Show(
                $"¿Eliminar a {Seleccionado.NombreCompleto}?\n\nEsto también eliminará sus registros como Cliente o Vendedor.",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                bool ok = _service.EliminarPersona(Seleccionado.PerId);
                if (!ok)
                {
                    MessageBox.Show("No se pudo eliminar la persona.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    CargarPersonas();
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
                CargarPersonas();
                return;
            }

            Personas = new ObservableCollection<Persona>(_service.Buscar(SearchTerm));
        }
    }
}