using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    public partial class Cliente : ObservableObject
    {
        [ObservableProperty]
        private decimal perId;

        [ObservableProperty]
        private string perNombre;

        [ObservableProperty]
        private string perApellido;

        [ObservableProperty]
        private string perDireccion;

        [ObservableProperty]
        private string perTelefono;

        [ObservableProperty]
        private string perEmail;


    }
}
