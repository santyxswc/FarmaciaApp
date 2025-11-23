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

        [ObservableProperty]
        private int proStock;

        [ObservableProperty]
        private string proDescripcion;
    }
}
