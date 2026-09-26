/**
 * @file Cliente.cs
 * @brief Modelo de cliente.
 * @author Santiago Caicedo
 */
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Persona registrada como cliente (TBL_PERSONA + TBL_CLIENTE).
     *
     * Notifica sus cambios para que los formularios habiliten el botón Guardar.
     */
    public partial class Cliente : ObservableObject
    {
        /** Identificador de la persona (PER_ID). */
        [ObservableProperty]
        private decimal perId;

        /** Nombre. */
        [ObservableProperty]
        private string perNombre;

        /** Apellido. */
        [ObservableProperty]
        private string perApellido;

        /** Dirección. */
        [ObservableProperty]
        private string perDireccion;

        /** Teléfono. */
        [ObservableProperty]
        private string perTelefono;

        /** Correo electrónico. */
        [ObservableProperty]
        private string perEmail;

    }
}
