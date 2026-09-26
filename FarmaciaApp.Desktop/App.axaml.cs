/**
 * @file App.axaml.cs
 * @brief Aplicación Avalonia.
 * @author Santiago Caicedo
 */
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Desktop.Views;
using Microsoft.Extensions.Configuration;

namespace FarmaciaApp.Desktop
{
    /**
     * @brief Carga los estilos globales, la configuración y la ventana de inicio de sesión.
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
         * @brief Lee appsettings.json, configura la conexión y abre el inicio de sesión.
         *
         * Si appsettings.json no existe, la ventana de inicio de sesión lo informa al intentar entrar.
         */
        public override void OnFrameworkInitializationCompleted()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
            DbConfig.Initialize(configuration);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnLastWindowClose;
                desktop.MainWindow = new LoginView();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
