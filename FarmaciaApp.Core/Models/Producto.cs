using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FarmaciaApp.Core.Models
{
    public partial class Producto : ObservableObject
    {
        public int ProId { get; set; }

        [ObservableProperty]
        private string proNombre;

        [ObservableProperty]
        private decimal proPrecio;

        public const int StockMinimo = 20;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StockBajo))]
        private int proStock;

        public bool StockBajo => ProStock <= StockMinimo;

        [ObservableProperty]
        private string proDescripcion;
    }
}
