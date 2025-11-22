using CommunityToolkit.Mvvm.ComponentModel;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

using FarmaciaApp.UI.ViewModels;
using System.Windows.Controls;

namespace FarmaciaApp.UI.Views
{
    public partial class ReclamosView : UserControl
    {
        public ReclamosView()
        {
            InitializeComponent();
            // Asignación crucial del DataContext
            DataContext = new ReclamosViewModel();
        }
    }
}
