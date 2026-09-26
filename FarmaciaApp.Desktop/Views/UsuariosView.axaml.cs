/**
 * @file UsuariosView.axaml.cs
 * @brief Vista de la administración de usuarios.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista de la administración de usuarios.
     */
    public partial class UsuariosView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public UsuariosView()
        {
            InitializeComponent();
            DataContext = new UsuariosViewModel();
        }
    }
}
