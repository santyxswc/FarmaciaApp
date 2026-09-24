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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            DbConfig.Initialize(configuration);
            var login = new LoginView();
            login.Show();
        }
    }
}
