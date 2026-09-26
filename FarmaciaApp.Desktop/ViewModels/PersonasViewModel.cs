using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
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

        public IAsyncRelayCommand AgregarCommand { get; }
        public IAsyncRelayCommand EditarCommand { get; }
        public IAsyncRelayCommand EliminarCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public IRelayCommand BuscarCommand { get; }

        public PersonasViewModel()
        {
            _service = new PersonaService();

            AgregarCommand = new AsyncRelayCommand(AbrirAgregar);
            EditarCommand = new AsyncRelayCommand(AbrirEditar, () => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(Eliminar, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(Cargar);
            BuscarCommand = new RelayCommand(Buscar);

            Cargar();
        }

        partial void OnSeleccionadoChanged(Persona value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        private void Cargar() => Ejecutar(() => Personas = new ObservableCollection<Persona>(_service.ObtenerPersonas()));

        private void Ejecutar(Action accion)
        {
            try
            {
                accion();
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error("Error: " + ex.Message, "Error de Base de Datos");
            }
        }

        private async Task AbrirAgregar()
        {
            var window = new AgregarEditarPersonaView();
            window.DataContext = new AgregarEditarPersonaViewModel(window);
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarPersonaView();
            var vm = new AgregarEditarPersonaViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        private async Task Eliminar()
        {
            if (Seleccionado == null) return;

            if (!await Dialogs.Confirm($"¿Eliminar a {Seleccionado.NombreCompleto}?\n\nEsto también eliminará sus registros como Cliente o Vendedor.")) return;

            try
            {
                if (!_service.EliminarPersona(Seleccionado.PerId))
                    await Dialogs.Error("No se pudo eliminar el registro.");
                else
                    Cargar();
            }
            catch (Exception ex)
            {
                await Dialogs.Error("Error: " + ex.Message);
            }
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Cargar();
                return;
            }

            Ejecutar(() => Personas = new ObservableCollection<Persona>(_service.Buscar(SearchTerm)));
        }
    }
}
