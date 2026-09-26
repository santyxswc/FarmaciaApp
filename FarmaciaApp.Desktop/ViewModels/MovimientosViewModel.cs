/**
 * @file MovimientosViewModel.cs
 * @brief Lógica del registro de movimientos.
 * @author Santiago Caicedo
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Collections.ObjectModel;

namespace FarmaciaApp.Desktop.ViewModels
{
    /**
     * @brief Consulta los movimientos con filtros por usuario, fechas y texto.
     */
    public partial class MovimientosViewModel : ObservableObject
    {
        private readonly MovimientoService _service = new();

        /** Usuarios para el filtro; el Id 0 representa a todos. */
        public List<Seleccion> Usuarios { get; }

        /** Usuario elegido en el filtro. */
        [ObservableProperty]
        private Seleccion usuarioSeleccionado;

        /** Fecha inicial. */
        [ObservableProperty]
        private DateTime? desde = DateTime.Today.AddDays(-6);

        /** Fecha final. */
        [ObservableProperty]
        private DateTime? hasta = DateTime.Today;

        /** Texto a buscar en la acción o el detalle. */
        [ObservableProperty]
        private string texto;

        /** Cantidad de movimientos encontrados. */
        [ObservableProperty]
        private string resumen;

        /** Error de la consulta. */
        [ObservableProperty]
        private string errorMessage;

        /** Movimientos encontrados. */
        public ObservableCollection<Movimiento> Movimientos { get; } = new();

        /** Aplica los filtros. */
        public IRelayCommand FiltrarCommand { get; }
        /** Vuelve a los filtros por defecto. */
        public IRelayCommand LimpiarCommand { get; }

        /**
         * @brief Carga los usuarios y los movimientos de los últimos 7 días.
         */
        public MovimientosViewModel()
        {
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

        /**
         * @brief Consulta los movimientos con los filtros actuales.
         */
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
