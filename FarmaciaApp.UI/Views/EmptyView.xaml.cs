using System.Windows.Controls;

namespace FarmaciaApp.UI.Views
{
    public partial class EmptyView : UserControl
    {
        public string Message { get; set; }

        public EmptyView(string title)
        {
            InitializeComponent();
            Message = $"Vista de {title} en construcción...";
            DataContext = this;
        }
    }
}
