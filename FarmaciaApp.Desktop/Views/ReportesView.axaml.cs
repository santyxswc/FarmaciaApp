using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    public partial class ReportesView : UserControl
    {
        public ReportesView()
        {
            InitializeComponent();
            DataContext = new ReportesViewModel();
        }
    }
}
