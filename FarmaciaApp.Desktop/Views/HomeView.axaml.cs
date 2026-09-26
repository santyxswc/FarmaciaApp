/**
 * @file HomeView.axaml.cs
 * @brief Vista de inicio con las tarjetas de acceso rápido.
 * @author Santiago Caicedo
 */
using Avalonia.Controls;

namespace FarmaciaApp.Desktop.Views
{
    /**
     * @brief Vista de inicio con las tarjetas de acceso rápido.
     */
    public partial class HomeView : UserControl
    {
        /**
         * @brief Crea la vista y quita las tarjetas de administración si el usuario no es administrador.
         */
        public HomeView()
        {
            InitializeComponent();

            if (!FarmaciaApp.Core.Sesion.EsAdmin)
            {
                rejilla.Children.Remove(tarjetaReportes);
                rejilla.Children.Remove(tarjetaMovimientos);
                rejilla.Children.Remove(tarjetaUsuarios);
            }

            SizeChanged += (_, e) => AjustarTarjetas(e.NewSize.Width);
        }

        /**
         * @brief Ajusta las columnas y el alto de las tarjetas al ancho disponible.
         * @param anchoTotal Ancho de la vista
         *
         * 4 columnas en pantallas anchas, 2 en medianas y 1 en angostas; el alto va de 110 a 170 px.
         */
        private void AjustarTarjetas(double anchoTotal)
        {
            double disponible = anchoTotal - 40;
            int columnas = disponible >= 760 ? 4 : disponible >= 380 ? 2 : 1;
            rejilla.Columns = columnas;

            double anchoTarjeta = disponible / columnas - 20;
            double alto = Math.Clamp(anchoTarjeta * 0.55, 110, 170);
            foreach (var tarjeta in rejilla.Children)
                tarjeta.Height = alto;
        }
    }
}
