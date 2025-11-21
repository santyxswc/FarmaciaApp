using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Controls;

namespace FarmaciaApp.UI.Services
{
    public class NavigationService : ObservableObject
    {
        private UserControl _currentView;

        public UserControl CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public void Navigate(UserControl view)
        {
            CurrentView = view;
        }
    }
}
