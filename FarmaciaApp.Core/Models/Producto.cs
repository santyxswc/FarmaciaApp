/**
 * @file Producto.cs
 * @brief Modelo de producto.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Medicamento o artículo del catálogo (TBL_PRODUCTO).
     *
     * Notifica sus cambios para validar el formulario mientras se escribe.
     */
    public partial class Producto : ObservableObject
    {
        /** Identificador (PRO_ID). */
        public int ProId { get; set; }

        /** Nombre. */
        [ObservableProperty]
        private string proNombre;

        /** Precio de venta con IVA incluido. */
        [ObservableProperty]
        private decimal proPrecio;

        /** Unidades desde las que el producto se marca con stock bajo. */
        public const int StockMinimo = 20;

        /** Unidades disponibles. */
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StockBajo))]
        private int proStock;

        /** Indica si el stock está en el mínimo o por debajo. */
        public bool StockBajo => ProStock <= StockMinimo;

        /** Descripción. */
        [ObservableProperty]
        private string proDescripcion;
    }
}
