using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using FarmaciaApp.UI.ViewModels;

namespace FarmaciaApp.UI.Views
{
    public partial class ProveedoresView : UserControl
    {
        public ProveedoresView()
        {
            InitializeComponent();
            DataContext = new ProveedoresViewModel();
        }
    }
}