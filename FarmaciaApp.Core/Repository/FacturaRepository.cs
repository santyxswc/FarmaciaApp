using Dapper;
using FarmaciaApp.Core.Database;
using FarmaciaApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FarmaciaApp.Core.Repositories
{
    public class FacturaRepository
    {
        private const string FacturaSelectSql = @"
            SELECT 
                F.FAC_NUM_FACTURA AS FacNumFactura,
                F.FAC_FECHA AS FacFecha,
                F.FAC_SUBTOTAL AS FacSubtotal,
                F.FAC_IVA AS FacIva,
                F.FAC_TOTAL AS FacTotal,
                F.CLI_ID AS CliId,
                F.VEN_ID AS VenId,
                F.PAG_ID AS PagId,
                P_CLI.PER_NOMBRE || ' ' || P_CLI.PER_APELLIDO AS ClienteNombre,
                P_VEN.PER_NOMBRE || ' ' || P_VEN.PER_APELLIDO AS VendedorNombre,
                PG.PAG_METODO AS MetodoPago
            FROM 
                TBL_FACTURA F
            INNER JOIN TBL_CLIENTE C ON F.CLI_ID = C.CLI_ID
            INNER JOIN TBL_PERSONA P_CLI ON C.PER_ID = P_CLI.PER_ID
            INNER JOIN TBL_VENDEDOR V ON F.VEN_ID = V.VEN_ID
            INNER JOIN TBL_PERSONA P_VEN ON V.PER_ID = P_VEN.PER_ID
            LEFT JOIN TBL_PAGO PG ON F.PAG_ID = PG.PAG_ID
            ";

        // Mayor descuento de las promociones vigentes del producto (0 si no tiene)
        private const string DescuentoActivoSql = @"
            NVL((SELECT MAX(PR.PRM_DESCUENTO)
                 FROM PROMO_PRODU PP
                 INNER JOIN TBL_PROMOCION PR ON PR.PRM_ID = PP.PRM_ID
                 WHERE PP.PRO_ID = P.PRO_ID
                   AND SYSDATE BETWEEN PR.PRM_FECHA_INI AND PR.PRM_FECHA_FIN), 0)";

        public IEnumerable<Factura> GetAll()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = FacturaSelectSql + " ORDER BY F.FAC_NUM_FACTURA DESC";
                return db.Query<Factura>(sql);
            }
        }

        public Factura GetById(decimal numero)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = FacturaSelectSql + " WHERE F.FAC_NUM_FACTURA = :Numero";
                return db.QueryFirstOrDefault<Factura>(sql, new { Numero = numero });
            }
        }

        public List<FacturaProductoDetalle> GetItems(decimal numero)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    SELECT FP.PRO_ID AS ProId,
                           P.PRO_NOMBRE AS ProNombre,
                           FP.CANTIDAD AS Cantidad,
                           FP.PRECIO_UNITARIO AS PrecioUnitario,
                           FP.SUBTOTAL_LINEA AS SubtotalLinea
                    FROM FACTU_PRODUC FP
                    INNER JOIN TBL_PRODUCTO P ON P.PRO_ID = FP.PRO_ID
                    WHERE FP.FAC_NUM_FACTURA = :Numero
                    ORDER BY P.PRO_NOMBRE";
                return db.Query<FacturaProductoDetalle>(sql, new { Numero = numero }).ToList();
            }
        }

        public IEnumerable<Factura> Search(string term)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = FacturaSelectSql + @"
                    WHERE LOWER(P_CLI.PER_NOMBRE) LIKE LOWER(:Term) 
                       OR LOWER(P_CLI.PER_APELLIDO) LIKE LOWER(:Term)
                       OR LOWER(P_VEN.PER_NOMBRE) LIKE LOWER(:Term) 
                       OR LOWER(P_VEN.PER_APELLIDO) LIKE LOWER(:Term)
                       OR TO_CHAR(F.FAC_NUM_FACTURA) LIKE :TermNumber
                    ORDER BY F.FAC_NUM_FACTURA DESC";

                return db.Query<Factura>(sql, new
                {
                    Term = $"%{term}%",
                    TermNumber = $"{term}%"
                });
            }
        }

        public IEnumerable<Seleccion> GetClientes()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.Query<Seleccion>(@"
                    SELECT C.CLI_ID AS Id, P.PER_NOMBRE || ' ' || P.PER_APELLIDO AS Nombre
                    FROM TBL_CLIENTE C INNER JOIN TBL_PERSONA P ON P.PER_ID = C.PER_ID
                    ORDER BY P.PER_NOMBRE, P.PER_APELLIDO");
            }
        }

        public IEnumerable<Seleccion> GetVendedores()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                return db.Query<Seleccion>(@"
                    SELECT V.VEN_ID AS Id, P.PER_NOMBRE || ' ' || P.PER_APELLIDO AS Nombre
                    FROM TBL_VENDEDOR V INNER JOIN TBL_PERSONA P ON P.PER_ID = V.PER_ID
                    ORDER BY P.PER_NOMBRE, P.PER_APELLIDO");
            }
        }

        public IEnumerable<ProductoVenta> GetProductosParaVenta()
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                string sql = @"
                    SELECT P.PRO_ID AS ProId,
                           P.PRO_NOMBRE AS ProNombre,
                           P.PRO_PRECIO AS PrecioBase,
                           P.PRO_STOCK AS Stock,
                           " + DescuentoActivoSql + @" AS Descuento
                    FROM TBL_PRODUCTO P
                    ORDER BY P.PRO_NOMBRE";
                return db.Query<ProductoVenta>(sql);
            }
        }

        // Registra la venta completa en una transaccion: pago, factura, lineas y descuento de stock.
        // Los precios se toman de la base de datos (con la promocion vigente), no de la pantalla.
        public decimal Insert(decimal cliId, decimal venId, string metodoPago, IEnumerable<FacturaProductoDetalle> items)
        {
            using (IDbConnection db = OracleDbConnection.GetConnection())
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        var lineas = new List<FacturaProductoDetalle>();
                        foreach (var item in items)
                        {
                            // FOR UPDATE bloquea el producto hasta el commit, para que dos ventas no gasten el mismo stock
                            var producto = db.QueryFirstOrDefault<ProductoVenta>(@"
                                SELECT PRO_ID AS ProId, PRO_NOMBRE AS ProNombre, PRO_PRECIO AS PrecioBase, PRO_STOCK AS Stock
                                FROM TBL_PRODUCTO WHERE PRO_ID = :Id FOR UPDATE", new { Id = item.ProId }, tran);

                            if (producto == null)
                                throw new InvalidOperationException($"El producto {item.ProId} ya no existe.");
                            if (producto.Stock < item.Cantidad)
                                throw new InvalidOperationException($"Stock insuficiente de {producto.ProNombre}: hay {producto.Stock} y se piden {item.Cantidad}.");

                            producto.Descuento = db.ExecuteScalar<decimal>(
                                "SELECT " + DescuentoActivoSql + " FROM TBL_PRODUCTO P WHERE P.PRO_ID = :Id",
                                new { Id = item.ProId }, tran);

                            lineas.Add(new FacturaProductoDetalle
                            {
                                ProId = producto.ProId,
                                ProNombre = producto.ProNombre,
                                Cantidad = item.Cantidad,
                                PrecioUnitario = producto.PrecioFinal,
                                SubtotalLinea = producto.PrecioFinal * item.Cantidad
                            });
                        }

                        decimal total = lineas.Sum(l => l.SubtotalLinea);
                        var (subtotal, iva) = Factura.DesglosarIva(total);

                        decimal pagId = db.ExecuteScalar<decimal>("SELECT NVL(MAX(PAG_ID), 0) + 1 FROM TBL_PAGO", null, tran);
                        db.Execute(@"INSERT INTO TBL_PAGO (PAG_ID, PAG_METODO, PAG_FECHA, PAG_MONTO)
                                     VALUES (:PagId, :Metodo, SYSDATE, :Total)",
                                   new { PagId = pagId, Metodo = metodoPago, Total = total }, tran);

                        decimal numero = db.ExecuteScalar<decimal>("SELECT NVL(MAX(FAC_NUM_FACTURA), 1000) + 1 FROM TBL_FACTURA", null, tran);
                        db.Execute(@"INSERT INTO TBL_FACTURA (FAC_NUM_FACTURA, FAC_FECHA, FAC_SUBTOTAL, FAC_IVA, FAC_TOTAL, CLI_ID, VEN_ID, PAG_ID)
                                     VALUES (:Numero, SYSDATE, :Subtotal, :Iva, :Total, :CliId, :VenId, :PagId)",
                                   new { Numero = numero, Subtotal = subtotal, Iva = iva, Total = total, CliId = cliId, VenId = venId, PagId = pagId }, tran);

                        foreach (var linea in lineas)
                        {
                            db.Execute(@"INSERT INTO FACTU_PRODUC (FAC_NUM_FACTURA, PRO_ID, CANTIDAD, PRECIO_UNITARIO, SUBTOTAL_LINEA)
                                         VALUES (:Numero, :ProId, :Cantidad, :PrecioUnitario, :SubtotalLinea)",
                                       new { Numero = numero, linea.ProId, linea.Cantidad, linea.PrecioUnitario, linea.SubtotalLinea }, tran);
                            db.Execute("UPDATE TBL_PRODUCTO SET PRO_STOCK = PRO_STOCK - :Cantidad WHERE PRO_ID = :ProId",
                                       new { linea.Cantidad, linea.ProId }, tran);
                        }

                        tran.Commit();
                        return numero;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
