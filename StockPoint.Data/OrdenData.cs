using Microsoft.Data.SqlClient;
using StockPoint.Domain;
using StockPoint.Data.Interfaces;

namespace StockPoint.Data
{
    public class OrdenData : IOrdenData
    {
        private readonly ConexionBD _conexion;
        private readonly IProductoData _productoData;

        public OrdenData(ConexionBD conexion, IProductoData productoData)
        {
            _conexion = conexion;
            _productoData = productoData;
        }

        public int ProcesarOrden(PosOrden orden)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // 1. Insertar cabecera de la orden
                int numeroOrden = InsertarCabecera(orden, connection, transaction);

                // 2. Insertar cada línea de detalle y 3. descontar el stock correspondiente
                foreach (var detalle in orden.Detalles)
                {
                    InsertarDetalle(numeroOrden, detalle, connection, transaction);

                    bool stockSuficiente = _productoData.ActualizarStock(
                        detalle.FkProducto, detalle.Cantidad, transaction, connection);

                    if (!stockSuficiente)
                    {
                        throw new InvalidOperationException(
                            $"No hay existencia suficiente para el producto {detalle.FkProducto}.");
                    }
                }

                // Si todo salió bien, se confirma la transacción completa
                transaction.Commit();
                return numeroOrden;
            }
            catch
            {
                // Si cualquier paso falla, se revierte TODO (cabecera, detalles y stock)
                transaction.Rollback();
                throw;
            }
        }

        private int InsertarCabecera(PosOrden orden, SqlConnection connection, SqlTransaction transaction)
        {
            const string sql = @"INSERT INTO Pos_Orden
                (fk_cliente, fk_empleado, fecha_orden, total_orden, impuesto)
                OUTPUT INSERTED.numero_orden
                VALUES (@cliente, @empleado, @fecha, @total, @impuesto)";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@cliente", orden.FkCliente);
            command.Parameters.AddWithValue("@empleado", orden.FkEmpleado);
            command.Parameters.AddWithValue("@fecha", orden.FechaOrden == default ? DateTime.Now : orden.FechaOrden);
            command.Parameters.AddWithValue("@total", orden.TotalOrden);
            command.Parameters.AddWithValue("@impuesto", orden.Impuesto);

            return (int)command.ExecuteScalar();
        }

        private void InsertarDetalle(int numeroOrden, PosOrdenDetalle detalle, SqlConnection connection, SqlTransaction transaction)
        {
            const string sql = @"INSERT INTO Pos_Orden_Detalle
                (fk_pos_orden, fk_producto, cantidad, impuesto, descuento, precio_unitario, precio_subtotal)
                VALUES (@orden, @producto, @cantidad, @impuesto, @descuento, @precioUnitario, @subtotal)";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@orden", numeroOrden);
            command.Parameters.AddWithValue("@producto", detalle.FkProducto);
            command.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
            command.Parameters.AddWithValue("@impuesto", detalle.Impuesto);
            command.Parameters.AddWithValue("@descuento", detalle.Descuento);
            command.Parameters.AddWithValue("@precioUnitario", detalle.PrecioUnitario);
            command.Parameters.AddWithValue("@subtotal", detalle.PrecioSubtotal);

            command.ExecuteNonQuery();
        }

        public PosOrden ObtenerPorNumero(int numeroOrden)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            // 1. Cabecera + nombre del cliente (JOIN)
            const string sqlCabecera = @"SELECT o.numero_orden, o.fk_cliente, o.fk_empleado, o.fecha_orden,
                                                 o.total_orden, o.impuesto,
                                                 c.nombre + ' ' + c.apellidos AS nombre_cliente
                                          FROM Pos_Orden o
                                          INNER JOIN Cliente c ON c.id = o.fk_cliente
                                          WHERE o.numero_orden = @numero";

            PosOrden orden = null;
            using (var command = new SqlCommand(sqlCabecera, connection))
            {
                command.Parameters.AddWithValue("@numero", numeroOrden);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    orden = new PosOrden
                    {
                        NumeroOrden = reader.GetInt32(0),
                        FkCliente = reader.GetInt32(1),
                        FkEmpleado = reader.GetInt32(2),
                        FechaOrden = reader.GetDateTime(3),
                        TotalOrden = reader.GetDecimal(4),
                        Impuesto = reader.GetDecimal(5),
                        NombreCliente = reader.GetString(6)
                    };
                }
            }

            if (orden == null) return null;

            // 2. Detalles + nombre/código del producto (JOIN), para que el WPF no necesite otro llamado
            const string sqlDetalle = @"SELECT d.fk_pos_orden, d.fk_producto, d.cantidad, d.impuesto,
                                                d.descuento, d.precio_unitario, d.precio_subtotal,
                                                p.nombre_etiqueta, p.codigo_barra
                                         FROM Pos_Orden_Detalle d
                                         INNER JOIN Producto p ON p.product_id = d.fk_producto
                                         WHERE d.fk_pos_orden = @numero";

            using var commandDetalle = new SqlCommand(sqlDetalle, connection);
            commandDetalle.Parameters.AddWithValue("@numero", numeroOrden);

            using var readerDetalle = commandDetalle.ExecuteReader();
            while (readerDetalle.Read())
            {
                orden.Detalles.Add(new PosOrdenDetalle
                {
                    FkPosOrden = readerDetalle.GetInt32(0),
                    FkProducto = readerDetalle.GetInt32(1),
                    Cantidad = readerDetalle.GetInt32(2),
                    Impuesto = readerDetalle.GetDecimal(3),
                    Descuento = readerDetalle.GetDecimal(4),
                    PrecioUnitario = readerDetalle.GetDecimal(5),
                    PrecioSubtotal = readerDetalle.GetDecimal(6),
                    NombreProducto = readerDetalle.GetString(7),
                    CodigoProducto = readerDetalle.IsDBNull(8) ? null : readerDetalle.GetString(8)
                });
            }

            return orden;
        }
    }
}