/**
 * @file PersonasView.axaml.cs
 * @brief Vista del listado de personas.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using FarmaciaApp.Desktop.ViewModels;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista del listado de personas.
     */
    public partial class PersonasView : UserControl
    {
        /**
         * @brief Crea la vista con su ViewModel.
         */
        public PersonasView()
        {
            InitializeComponent();
            DataContext = new PersonasViewModel();
        }
    }
}
