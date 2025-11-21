using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    public class Producto
    {
        public int ProId { get; set; }
        public string ProNombre { get; set; }
        public decimal ProPrecio { get; set; }
        public int ProStock { get; set; }
        public string ProDescripcion { get; set; }
    }
}
