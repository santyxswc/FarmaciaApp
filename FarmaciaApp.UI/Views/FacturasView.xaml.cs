using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

using FarmaciaApp.UI.ViewModels;


namespace FarmaciaApp.UI.Views
{
    public partial class FacturasView : UserControl
    {
        public FacturasView()
        {
            InitializeComponent();
            // Asignación crucial del DataContext
            DataContext = new FacturasViewModel();
        }
    }
}
