using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.Views;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class UsuariosViewModel : ObservableObject
    {
        private readonly UsuarioService _service = new();

        [ObservableProperty]
        private ObservableCollection<Usuario> usuarios;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoEstado))]
        private Usuario seleccionado;

        public string TextoEstado => Seleccionado?.Activo == false ? "Activar" : "Desactivar";

        public IAsyncRelayCommand NuevoCommand { get; }
        public IAsyncRelayCommand CambiarEstadoCommand { get; }
        public IAsyncRelayCommand RestablecerClaveCommand { get; }
        public IRelayCommand RefreshCommand { get; }

        public UsuariosViewModel()
        {
            NuevoCommand = new AsyncRelayCommand(Nuevo);
            CambiarEstadoCommand = new AsyncRelayCommand(CambiarEstado, () => Seleccionado != null);
            RestablecerClaveCommand = new AsyncRelayCommand(RestablecerClave, () => Seleccionado != null);
            RefreshCommand = new RelayCommand(Cargar);
            Cargar();
        }

        partial void OnSeleccionadoChanged(Usuario value)
        {
            CambiarEstadoCommand.NotifyCanExecuteChanged();
            RestablecerClaveCommand.NotifyCanExecuteChanged();
        }

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

        private async Task Nuevo()
        {
            var ventana = new NuevoUsuarioView();
            ventana.DataContext = new NuevoUsuarioViewModel(ventana);
            if (await Dialogs.ShowForm(ventana)) Cargar();
        }

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

        private async Task RestablecerClave()
        {
            var ventana = new CambiarClaveView();
            ventana.DataContext = new CambiarClaveViewModel(ventana, Seleccionado);
            if (await Dialogs.ShowForm(ventana))
                await Dialogs.Info($"La contraseña de {Seleccionado.Login} fue restablecida.", "Contraseña restablecida");
        }
    }
}
