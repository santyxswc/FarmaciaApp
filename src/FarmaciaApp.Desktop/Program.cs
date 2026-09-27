/**
 * @file Program.cs
 * @brief Punto de entrada de la aplicación.
 * @author Santiago Caicedo
 *
 * Aplicación de escritorio multiplataforma (Windows, Linux y macOS) hecha con Avalonia.
 */
using Avalonia;
using System.Globalization;

namespace FarmaciaApp.Desktop
{
    /**
     * @brief Arranque de la aplicación de escritorio.
     */
    internal static class Program
    {
        /**
         * @brief Configura la cultura es-CO e inicia Avalonia.
         * @param args Argumentos de la línea de comandos
         */
        [STAThread]
        public static void Main(string[] args)
        {
            var cultura = FarmaciaApp.Core.Formato.Cultura;
            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;
            CultureInfo.CurrentCulture = cultura;
            CultureInfo.CurrentUICulture = cultura;

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        /**
         * @brief Configura Avalonia: plataforma, fuente Inter y trazas.
         * @return Constructor de la aplicación
         */
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
