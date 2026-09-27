/**
 * @file FabricaVistas.cs
 * @brief Crea vistas con su ViewModel resuelto por el contenedor de dependencias.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace FarmaciaApp.Desktop.Services
{
    /**
     * @brief Crea las vistas con su ViewModel.
     *
     * Los servicios del ViewModel los inyecta el contenedor; la ventana y el registro a editar
     * se pasan como argumentos.
     */
    public sealed class FabricaVistas
    {
        /** Contenedor de dependencias de la aplicación. */
        private readonly IServiceProvider _servicios;

        /**
         * @brief Crea la fábrica.
         * @param servicios Contenedor de dependencias
         */
        public FabricaVistas(IServiceProvider servicios) => _servicios = servicios;

        /**
         * @brief Crea una sección del área de contenido con su ViewModel.
         * @return Vista lista para mostrar
         */
        public Control Seccion<TView, TViewModel>() where TView : Control, new() =>
            new TView { DataContext = ActivatorUtilities.CreateInstance<TViewModel>(_servicios) };

        /**
         * @brief Crea una ventana y su ViewModel, que recibe la ventana como primer argumento.
         * @param argumentos Argumentos adicionales del ViewModel (por ejemplo, el registro a editar)
         * @return Ventana y ViewModel
         */
        public (TWindow Ventana, TViewModel ViewModel) Ventana<TWindow, TViewModel>(params object[] argumentos)
            where TWindow : Window, new()
        {
            var ventana = new TWindow();
            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_servicios, [ventana, .. argumentos]);
            ventana.DataContext = viewModel;
            return (ventana, viewModel);
        }
    }
}
