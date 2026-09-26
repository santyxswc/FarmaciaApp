using Avalonia.Controls;
using FarmaciaApp.Core;
using FarmaciaApp.Core.Services;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
            Title = $"Farmacia · {Sesion.Usuario?.Nombre}";

            // Cerrar la ventana tambien cierra el turno (si no se cerro ya con el boton)
            Closing += (_, _) =>
            {
                if (Sesion.Activa)
                    new UsuarioService().CerrarSesion("ventana cerrada");
            };
        }
    }
}
