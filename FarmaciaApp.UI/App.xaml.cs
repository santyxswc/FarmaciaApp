using FarmaciaApp.Core.Database;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows;
using FarmaciaApp.UI.Views;

namespace FarmaciaApp.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cargar appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Inicializar cadena de conexión
            DbConfig.Initialize(configuration);

            // Mostrar login
            var login = new LoginView();
            login.Show();
        }
    }
}
