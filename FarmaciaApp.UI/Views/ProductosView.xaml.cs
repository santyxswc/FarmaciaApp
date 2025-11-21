using System;
using System.Collections.Generic;
using System.Text;

using FarmaciaApp.UI.ViewModels;

namespace FarmaciaApp.UI.Views
{
    public partial class ProductosView
    {
        public ProductosView()
        {
            InitializeComponent();
            DataContext = new ProductosViewModel();
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
