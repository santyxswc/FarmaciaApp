/**
 * @file UsuariosViewModel.cs
 * @brief Lógica de la administración de usuarios.
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
     * @brief Lista, crea, activa, desactiva y restablece cuentas de usuario.
     */
    public partial class UsuariosViewModel : ObservableObject
    {
        private readonly UsuarioService _service = new();

        /** Usuarios registrados. */
        [ObservableProperty]
        private ObservableCollection<Usuario> usuarios;

        /** Usuario seleccionado en la tabla. */
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoEstado))]
        private Usuario seleccionado;

        /** Texto del botón de estado: Activar o Desactivar. */
        public string TextoEstado => Seleccionado?.Activo == false ? "Activar" : "Desactivar";

        /** Abre el formulario de nuevo usuario. */
        public IAsyncRelayCommand NuevoCommand { get; }
        /** Activa o desactiva el usuario seleccionado. */
        public IAsyncRelayCommand CambiarEstadoCommand { get; }
        /** Restablece la contraseña del usuario seleccionado. */
        public IAsyncRelayCommand RestablecerClaveCommand { get; }
        /** Vuelve a cargar la lista. */
        public IRelayCommand RefreshCommand { get; }

        /**
         * @brief Crea los comandos y carga los usuarios.
         */
        public UsuariosViewModel()
        {
            NuevoCommand = new AsyncRelayCommand(Nuevo);
            CambiarEstadoCommand = new AsyncRelayCommand(CambiarEstado, () => Seleccionado != null);
            RestablecerClaveCommand = new AsyncRelayCommand(RestablecerClave, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(Cargar);
            Cargar();
        }

        /**
         * @brief Habilita los botones que requieren un usuario seleccionado.
         * @param value Usuario seleccionado
         */
        partial void OnSeleccionadoChanged(Usuario value)
        {
            CambiarEstadoCommand.NotifyCanExecuteChanged();
            RestablecerClaveCommand.NotifyCanExecuteChanged();
        }

        /**
         * @brief Carga los usuarios.
         */
        private void Cargar()
        {
            try
            {
                Usuarios = new ObservableCollection<Usuario>(_service.ObtenerUsuarios());
            }
            catch (Exception ex)
            {
                _ = Dialogs.Error("Error al cargar usuarios: " + ex.Message);
            }
        }

        /**
         * @brief Abre el formulario de nuevo usuario y recarga la lista.
         */
        private async Task Nuevo()
        {
            var ventana = new NuevoUsuarioView();
            ventana.DataContext = new NuevoUsuarioViewModel(ventana);
            if (await Dialogs.ShowForm(ventana)) Cargar();
        }

        /**
         * @brief Pide confirmación y activa o desactiva el usuario seleccionado.
         */
        private async Task CambiarEstado()
        {
            var usuario = Seleccionado;
            bool activar = !usuario.Activo;
            string pregunta = activar
                ? $"¿Activar al usuario {usuario.Login}? Podrá volver a iniciar sesión."
                : $"¿Desactivar al usuario {usuario.Login}? No podrá iniciar sesión, pero su historial se conserva.";
            if (!await Dialogs.Confirm(pregunta)) return;

            try
            {
                _service.CambiarEstado(usuario.UsuId, activar);
                Cargar();
            }
            catch (Exception ex)
            {
                await Dialogs.Error(ex.Message);
            }
        }

        /**
         * @brief Abre el formulario para asignar una contraseña nueva.
         */
        private async Task RestablecerClave()
        {
            var ventana = new CambiarClaveView();
            ventana.DataContext = new CambiarClaveViewModel(ventana, Seleccionado);
            if (await Dialogs.ShowForm(ventana))
                await Dialogs.Info($"La contraseña de {Seleccionado.Login} fue restablecida.", "Contraseña restablecida");
        }
    }
}
