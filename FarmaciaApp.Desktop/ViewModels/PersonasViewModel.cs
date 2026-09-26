/**
 * @file PersonasViewModel.cs
 * @brief Lógica del listado de personas.
 * @author Santiago Caicedo
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Lista, busca y abre los formularios de personas.
     */
    public partial class PersonasViewModel : ObservableObject
    {
        private readonly PersonaService _service;

        /** Personas que se muestran. */
        [ObservableProperty]
        private ObservableCollection<Persona> personas;

        /** Registro seleccionado en la tabla. */
        [ObservableProperty]
        private Persona seleccionado;

        /** Texto de búsqueda. */
        [ObservableProperty]
        private string searchTerm;

        /** Abre el formulario para crear. */
        public IAsyncRelayCommand AgregarCommand { get; }
        /** Abre el formulario para editar el seleccionado. */
        public IAsyncRelayCommand EditarCommand { get; }
        /** Elimina el seleccionado después de confirmar. */
        public IAsyncRelayCommand EliminarCommand { get; }
        /** Vuelve a cargar la lista. */
        public IRelayCommand RefreshCommand { get; }
        /** Busca con el texto escrito. */
        public IRelayCommand BuscarCommand { get; }

        /**
         * @brief Crea los comandos y carga la lista.
         */
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

        /**
         * @brief Habilita Editar y Eliminar según la selección.
         * @param value Registro seleccionado
         */
        partial void OnSeleccionadoChanged(Persona value)
        {
            EditarCommand.NotifyCanExecuteChanged();
            EliminarCommand.NotifyCanExecuteChanged();
        }

        /**
         * @brief Carga todos los registros.
         */
        private void Cargar() => Ejecutar(() => Personas = new ObservableCollection<Persona>(_service.ObtenerPersonas()));

        /**
         * @brief Ejecuta una consulta y muestra el error si falla.
         * @param accion Consulta a ejecutar
         */
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

        /**
         * @brief Abre el formulario para crear y recarga la lista al guardar.
         */
        private async Task AbrirAgregar()
        {
            var window = new AgregarEditarPersonaView();
            window.DataContext = new AgregarEditarPersonaViewModel(window);
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        /**
         * @brief Abre el formulario con el registro seleccionado y recarga la lista al guardar.
         */
        private async Task AbrirEditar()
        {
            if (Seleccionado == null) return;

            var window = new AgregarEditarPersonaView();
            var vm = new AgregarEditarPersonaViewModel(window);
            vm.LoadFromModel(Seleccionado);
            window.DataContext = vm;
            if (await Dialogs.ShowForm(window)) Cargar();
        }

        /**
         * @brief Pide confirmación y elimina el registro seleccionado.
         */
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

        /**
         * @brief Busca con el texto escrito; si está vacío, muestra todos.
         */
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
