/**
 * @file Factura.cs
 * @brief Modelos de factura y de sus líneas.
 * @author Santiago Caicedo
 */
using System;
using System.Collections.Generic;

namespace FarmaciaApp.Core.Models
{
    /**
     * @brief Factura de venta con los nombres de cliente, vendedor y método de pago.
     */
    public class Factura
    {
        /** Tasa de IVA (19 %). Los precios de los productos ya la incluyen. */
        public const decimal TasaIva = 0.19m;

        /** Número de factura. */
        public decimal FacNumFactura { get; set; }
        /** Fecha y hora de la venta. */
        public DateTime FacFecha { get; set; }
        /** Total sin IVA. */
        public decimal FacSubtotal { get; set; }
        /** Valor del IVA. */
        public decimal FacIva { get; set; }
        /** Total pagado (con IVA). */
        public decimal FacTotal { get; set; }
        /** CLI_ID del cliente. */
        public decimal CliId { get; set; }
        /** VEN_ID del vendedor. */
        public decimal VenId { get; set; }
        /** PAG_ID del pago. */
        public decimal PagId { get; set; }
        /** Nombre completo del cliente. */
        public string ClienteNombre { get; set; }
        /** Nombre completo del vendedor. */
        public string VendedorNombre { get; set; }
        /** Método de pago (efectivo, tarjeta...). */
        public string MetodoPago { get; set; }
        /** Lineas de la factura; solo se cargan al pedir el detalle. */
        public List<FacturaProductoDetalle> Items { get; set; }

        /**
         * @brief Separa el IVA de un total que ya lo incluye.
         * @param total Total con IVA
         * @return Subtotal (total / 1.19, redondeado a 2 decimales) e IVA
         */
        public static (decimal Subtotal, decimal Iva) DesglosarIva(decimal total)
        {
            decimal subtotal = Math.Round(total / (1 + TasaIva), 2);
            return (subtotal, total - subtotal);
        }
    }
    /**
     * @brief Línea de una factura (FACTU_PRODUC).
     */
    public class FacturaProductoDetalle
    {
        /** Identificador del producto. */
        public decimal ProId { get; set; }
        /** Nombre del producto. */
        public string ProNombre { get; set; }
        /** Unidades vendidas. */
        public decimal Cantidad { get; set; }
        /** Precio cobrado por unidad (con IVA y promoción). */
        public decimal PrecioUnitario { get; set; }
        /** Cantidad por precio unitario. */
        public decimal SubtotalLinea { get; set; }
    }
}
