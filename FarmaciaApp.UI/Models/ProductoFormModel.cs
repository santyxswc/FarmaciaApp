using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;

namespace FarmaciaApp.UI.Models
{
    public class ProductoFormModel : IDataErrorInfo
    {
        public int ProId { get; set; }
        public string ProNombre { get; set; }
        public decimal ProPrecio { get; set; }
        public int ProStock { get; set; }
        public string ProDescripcion { get; set; }

        public string Error => null;

        public string this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case nameof(ProNombre):
                        if (string.IsNullOrWhiteSpace(ProNombre))
                            return "El nombre es obligatorio.";
                        break;

                    case nameof(ProPrecio):
                        if (ProPrecio <= 0)
                            return "El precio debe ser mayor a 0.";
                        break;

                    case nameof(ProStock):
                        if (ProStock < 0)
                            return "El stock no puede ser negativo.";
                        break;
                }

                return null;
            }
        }
    }
}
