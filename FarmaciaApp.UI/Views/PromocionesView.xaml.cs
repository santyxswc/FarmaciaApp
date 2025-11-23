using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Controls;
using FarmaciaApp.UI.ViewModels;

namespace FarmaciaApp.UI.Views
{
    public partial class PromocionesView : UserControl
    {
        public PromocionesView()
        {
            InitializeComponent();
            DataContext = new PromocionesViewModel();
        }
    }
}