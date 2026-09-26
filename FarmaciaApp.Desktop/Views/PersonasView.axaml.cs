using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
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
