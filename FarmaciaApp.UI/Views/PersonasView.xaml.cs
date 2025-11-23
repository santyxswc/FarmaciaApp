using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using FarmaciaApp.UI.ViewModels;

namespace FarmaciaApp.UI.Views
{
    public partial class PersonasView : UserControl
    {
        public PersonasView()
        {
            InitializeComponent();
            DataContext = new PersonasViewModel();
        }
    }
}