using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class ReclamosView : UserControl
    {
        public ReclamosView()
        {
            InitializeComponent();
            DataContext = new ReclamosViewModel();
        }
    }
}
