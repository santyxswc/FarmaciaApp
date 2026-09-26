/**
 * @file FacturaDetalleView.axaml.cs
 * @brief Ventana con el detalle de una factura.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Ventana con el detalle de una factura.
     */
    public partial class FacturaDetalleView : Window
    {
        /**
         * @brief Crea la ventana; su DataContext es la Factura.
         */
        public FacturaDetalleView()
        {
            InitializeComponent();
        }

        /**
         * @brief Cierra la ventana.
         * @param sender Boton
         * @param e Evento
         */
        private void Cerrar_Click(object sender, RoutedEventArgs e) => Close(false);
    }
}
