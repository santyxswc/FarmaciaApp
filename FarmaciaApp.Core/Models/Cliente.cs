using CommunityToolkit.Mvvm.ComponentModel; // <-- ¡Agregar este using!
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    // 1. La clase debe ser 'partial' y heredar de 'ObservableObject'
    public partial class Cliente : ObservableObject
    {
        // 2. Reemplazar las propiedades públicas por campos privados con [ObservableProperty]

        // Campos heredados de TBL_PERSONA
        [ObservableProperty]
        private decimal perId;        // ID

        [ObservableProperty]
        private string perNombre;     // Nombre

        [ObservableProperty]
        private string perApellido;   // Apellido

        [ObservableProperty]
        private string perDireccion;  // Dirección

        [ObservableProperty]
        private string perTelefono;   // Teléfono

        [ObservableProperty]
        private string perEmail;      // Email


    }
}
