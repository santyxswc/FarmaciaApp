/**
 * @file ReporteRepository.cs
 * @brief Consultas de los reportes de ventas.
 * @author Santiago Caicedo
 */
using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace FarmaciaApp.Core.Repositories
{
    /**
     * @brief Totales de ventas entre dos fechas (la fecha final no se incluye).
     */
    public class ReporteRepository
    {
        /**
         * @brief Calcula facturas, total y unidades vendidas.
         * @param desde Fecha inicial
         * @param hasta Fecha final (no incluida)
         * @return Resumen del periodo
         */
        public ResumenVentas GetResumen(DateTime desde, DateTime hasta)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                var resumen = db.QueryFirst<ResumenVentas>(@"
                    SELECT COUNT(*) AS Facturas, NVL(SUM(FAC_TOTAL), 0) AS Total
                    FROM TBL_FACTURA
                    WHERE FAC_FECHA >= :Desde AND FAC_FECHA < :Hasta", new { Desde = desde, Hasta = hasta });

                resumen.Unidades = db.ExecuteScalar<decimal>(@"
                    SELECT NVL(SUM(FP.CANTIDAD), 0)
                    FROM FACTU_PRODUC FP
                    INNER JOIN TBL_FACTURA F ON F.FAC_NUM_FACTURA = FP.FAC_NUM_FACTURA
                    WHERE F.FAC_FECHA >= :Desde AND F.FAC_FECHA < :Hasta", new { Desde = desde, Hasta = hasta });

                return resumen;
            }
        }

        /**
         * @brief Agrupa las ventas por vendedor.
         * @param desde Fecha inicial
         * @param hasta Fecha final (no incluida)
         * @return Vendedores ordenados por total vendido
         */
        public IEnumerable<VentaPorVendedor> GetVentasPorVendedor(DateTime desde, DateTime hasta)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.Query<VentaPorVendedor>(@"
                    SELECT P.PER_NOMBRE || ' ' || P.PER_APELLIDO AS Vendedor,
                           COUNT(*) AS Facturas,
                           SUM(F.FAC_TOTAL) AS Total
                    FROM TBL_FACTURA F
                    INNER JOIN TBL_VENDEDOR V ON V.VEN_ID = F.VEN_ID
                    INNER JOIN TBL_PERSONA P ON P.PER_ID = V.PER_ID
                    WHERE F.FAC_FECHA >= :Desde AND F.FAC_FECHA < :Hasta
                    GROUP BY P.PER_NOMBRE, P.PER_APELLIDO
                    ORDER BY Total DESC", new { Desde = desde, Hasta = hasta });
            }
        }

        /**
         * @brief Obtiene los 10 productos con más unidades vendidas.
         * @param desde Fecha inicial
         * @param hasta Fecha final (no incluida)
         * @return Productos ordenados por unidades
         */
        public IEnumerable<ProductoVendido> GetProductosMasVendidos(DateTime desde, DateTime hasta)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.Query<ProductoVendido>(@"
                    SELECT PR.PRO_NOMBRE AS Producto,
                           SUM(FP.CANTIDAD) AS Unidades,
                           SUM(FP.SUBTOTAL_LINEA) AS Total
                    FROM FACTU_PRODUC FP
                    INNER JOIN TBL_FACTURA F ON F.FAC_NUM_FACTURA = FP.FAC_NUM_FACTURA
                    INNER JOIN TBL_PRODUCTO PR ON PR.PRO_ID = FP.PRO_ID
                    WHERE F.FAC_FECHA >= :Desde AND F.FAC_FECHA < :Hasta
                    GROUP BY PR.PRO_NOMBRE
                    ORDER BY Unidades DESC, Total DESC
                    FETCH FIRST 10 ROWS ONLY", new { Desde = desde, Hasta = hasta });
            }
        }
    }
}
