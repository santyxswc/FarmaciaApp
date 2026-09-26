using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Desktop.Views;
using Microsoft.Extensions.Configuration;

namespace FarmaciaApp.Desktop
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

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
