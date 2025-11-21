using System.Windows;
using FarmaciaApp.UI.ViewModels;

namespace FarmaciaApp.UI.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }
    }
}
