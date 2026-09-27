/**
 * @file App.axaml.cs
 * @brief Aplicación Avalonia y raíz de composición.
 * @author Santiago Caicedo
 */
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FarmaciaApp.Desktop.Services;
using FarmaciaApp.Desktop.ViewModels;
using FarmaciaApp.Desktop.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FarmaciaApp.Desktop
{
    /**
     * @brief Carga los estilos globales, compone los servicios y abre el inicio de sesión.
     */
    public partial class App : Application
    {
        /**
         * @brief Carga App.axaml (estilos y recursos).
         */
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /**
         * @brief Lee appsettings.json, construye el contenedor de dependencias y abre el inicio de sesión.
         *
         * Si appsettings.json no existe, la ventana de inicio de sesión lo informa al intentar entrar.
         */
        public override void OnFrameworkInitializationCompleted()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var servicios = new ServiceCollection()
                .AgregarFarmacia(configuration.GetConnectionString("OracleConnection"))
                .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnLastWindowClose;
                desktop.MainWindow = servicios.GetRequiredService<FabricaVistas>()
                    .Ventana<LoginView, LoginViewModel>().Ventana;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
