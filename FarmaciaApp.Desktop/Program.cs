using Avalonia;
using System.Globalization;

namespace FarmaciaApp.Desktop
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Pesos colombianos y fechas dd/MM/yyyy, sin importar el idioma del sistema
            var cultura = new CultureInfo("es-CO");
            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;
            CultureInfo.CurrentCulture = cultura;
            CultureInfo.CurrentUICulture = cultura;

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
