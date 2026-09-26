using Avalonia.Controls;

namespace FarmaciaApp.Desktop.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            SizeChanged += (_, e) => AjustarTarjetas(e.NewSize.Width);
        }

        // 4 columnas en pantallas anchas, 2 en medianas y 1 en angostas;
        // el alto crece con el ancho de la tarjeta, entre 110 y 170 px
        private void AjustarTarjetas(double anchoTotal)
        {
            double disponible = anchoTotal - 40; // margen de la vista
            int columnas = disponible >= 760 ? 4 : disponible >= 380 ? 2 : 1;
            rejilla.Columns = columnas;

            double anchoTarjeta = disponible / columnas - 20; // margen de cada tarjeta
            double alto = Math.Clamp(anchoTarjeta * 0.55, 110, 170);
            foreach (var tarjeta in rejilla.Children)
                tarjeta.Height = alto;
        }
    }
}
