using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    public partial class MovimientosViewModel : ObservableObject
    {
        private readonly MovimientoService _service = new();

        public List<Seleccion> Usuarios { get; }

        [ObservableProperty]
        private Seleccion usuarioSeleccionado;

        [ObservableProperty]
        private DateTime? desde = DateTime.Today.AddDays(-6);

        [ObservableProperty]
        private DateTime? hasta = DateTime.Today;

        [ObservableProperty]
        private string texto;

        [ObservableProperty]
        private string resumen;

        [ObservableProperty]
        private string errorMessage;

        public ObservableCollection<Movimiento> Movimientos { get; } = new();

        public IRelayCommand FiltrarCommand { get; }
        public IRelayCommand LimpiarCommand { get; }

        public MovimientosViewModel()
        {
            // Id 0 = todos los usuarios
            Usuarios = new List<Seleccion> { new() { Id = 0, Nombre = "Todos los usuarios" } };
            Usuarios.AddRange(new UsuarioService().ObtenerUsuarios()
                .Select(u => new Seleccion { Id = u.UsuId, Nombre = $"{u.Login} ({u.Nombre})" }));
            UsuarioSeleccionado = Usuarios[0];

            FiltrarCommand = new RelayCommand(Filtrar);
            LimpiarCommand = new RelayCommand(() =>
            {
                UsuarioSeleccionado = Usuarios[0];
                Desde = DateTime.Today.AddDays(-6);
                Hasta = DateTime.Today;
                Texto = null;
                Filtrar();
            });
            Filtrar();
        }

        private void Filtrar()
        {
            ErrorMessage = null;
            try
            {
                decimal? usuId = UsuarioSeleccionado == null || UsuarioSeleccionado.Id == 0 ? null : UsuarioSeleccionado.Id;
                var lista = _service.Buscar(Desde ?? DateTime.Today, Hasta ?? DateTime.Today, usuId, Texto).ToList();

                Movimientos.Clear();
                foreach (var m in lista) Movimientos.Add(m);
                Resumen = lista.Count >= 500
                    ? "Mostrando los 500 movimientos más recientes; ajusta los filtros para ver más"
                    : $"{lista.Count} movimiento(s)";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
