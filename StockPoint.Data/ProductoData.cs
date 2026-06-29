using Microsoft.Data.SqlClient;
using StockPoint.Domain;
using StockPoint.Data.Interfaces;
using System.Linq;
namespace StockPoint.Data
{
    public class ProductoData : IProductoData
    {
        private readonly ConexionBD _conexion;
        private readonly ICategoriaData _categoriaData;

        public ProductoData(ConexionBD conexion, ICategoriaData categoriaData)
        {
            _conexion = conexion;
            _categoriaData = categoriaData;
        }

        public List<Producto> ObtenerTodos()
        {
            var lista = new List<Producto>();
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT product_id, nombre_etiqueta, descripcion, existencia_en_stock,
                                         existencia_limite_para_alerta, precio_neto, precio_minimo,
                                         precio_minimo_con_impuesto, tiene_impuesto, impuesto_valor_agregado,
                                         impuesto_local1, impuesto_local2, puede_venderse, puede_comprarse,
                                         codigo_barra
                                  FROM Producto";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearProducto(reader));
            }
            reader.Close();

            // Se llenan las categorías después de cerrar el reader (misma conexión, nuevo command)
            foreach (var producto in lista)
            {
                producto.Categorias = _categoriaData.ObtenerPorProducto(producto.ProductId, connection);
            }

            return lista;
        }

        public Producto ObtenerPorId(int id)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT product_id, nombre_etiqueta, descripcion, existencia_en_stock,
                                         existencia_limite_para_alerta, precio_neto, precio_minimo,
                                         precio_minimo_con_impuesto, tiene_impuesto, impuesto_valor_agregado,
                                         impuesto_local1, impuesto_local2, puede_venderse, puede_comprarse,
                                         codigo_barra
                                  FROM Producto WHERE product_id = @id";

            Producto producto = null;
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    producto = MapearProducto(reader);
                }
            }

            if (producto != null)
            {
                producto.Categorias = _categoriaData.ObtenerPorProducto(producto.ProductId, connection);
            }

            return producto;
        }

        // Requerimiento I.2.b: "buscar el producto" al agregarlo a la orden
        public List<Producto> BuscarPorNombreOCodigo(string termino)
        {
            var lista = new List<Producto>();
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT product_id, nombre_etiqueta, descripcion, existencia_en_stock,
                                         existencia_limite_para_alerta, precio_neto, precio_minimo,
                                         precio_minimo_con_impuesto, tiene_impuesto, impuesto_valor_agregado,
                                         impuesto_local1, impuesto_local2, puede_venderse, puede_comprarse,
                                         codigo_barra
                                  FROM Producto
                                  WHERE puede_venderse = 1
                                    AND (nombre_etiqueta LIKE @termino OR codigo_barra LIKE @termino)";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@termino", $"%{termino}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapearProducto(reader));
            }
            return lista;
        }

        public int Crear(Producto producto)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string sql = @"INSERT INTO Producto
                    (nombre_etiqueta, descripcion, existencia_en_stock, existencia_limite_para_alerta,
                     precio_neto, precio_minimo, precio_minimo_con_impuesto, tiene_impuesto,
                     impuesto_valor_agregado, impuesto_local1, impuesto_local2, puede_venderse,
                     puede_comprarse, codigo_barra)
                    OUTPUT INSERTED.product_id
                    VALUES
                    (@nombre, @descripcion, @stock, @limiteAlerta, @precioNeto, @precioMinimo,
                     @precioMinimoConImp, @tieneImpuesto, @iva, @impLocal1, @impLocal2,
                     @puedeVenderse, @puedeComprarse, @codigoBarra)";

                using var command = new SqlCommand(sql, connection, transaction);
                AgregarParametros(command, producto);
                int nuevoId = (int)command.ExecuteScalar();

                var idsCategoria = producto.Categorias?.Select(c => c.IdCategoria).ToList();
                _categoriaData.AsignarCategorias(nuevoId, idsCategoria, connection, transaction);

                transaction.Commit();
                return nuevoId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Actualizar(Producto producto)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string sql = @"UPDATE Producto SET
                    nombre_etiqueta = @nombre, descripcion = @descripcion,
                    existencia_en_stock = @stock, existencia_limite_para_alerta = @limiteAlerta,
                    precio_neto = @precioNeto, precio_minimo = @precioMinimo,
                    precio_minimo_con_impuesto = @precioMinimoConImp, tiene_impuesto = @tieneImpuesto,
                    impuesto_valor_agregado = @iva, impuesto_local1 = @impLocal1,
                    impuesto_local2 = @impLocal2, puede_venderse = @puedeVenderse,
                    puede_comprarse = @puedeComprarse, codigo_barra = @codigoBarra
                    WHERE product_id = @id";

                using var command = new SqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@id", producto.ProductId);
                AgregarParametros(command, producto);
                bool actualizado = command.ExecuteNonQuery() > 0;

                // Se reemplazan las categorías: se borran las anteriores y se insertan las nuevas
                _categoriaData.EliminarCategoriasDeProducto(producto.ProductId, connection, transaction);
                var idsCategoria = producto.Categorias?.Select(c => c.IdCategoria).ToList();
                _categoriaData.AsignarCategorias(producto.ProductId, idsCategoria, connection, transaction);

                transaction.Commit();
                return actualizado;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Eliminar(int id)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                _categoriaData.EliminarCategoriasDeProducto(id, connection, transaction);

                using var command = new SqlCommand("DELETE FROM Producto WHERE product_id = @id", connection, transaction);
                command.Parameters.AddWithValue("@id", id);
                bool eliminado = command.ExecuteNonQuery() > 0;

                transaction.Commit();
                return eliminado;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool ActualizarStock(int productId, int cantidadVendida, SqlTransaction transaction, SqlConnection connection)
        {
            const string sql = @"UPDATE Producto
                                  SET existencia_en_stock = existencia_en_stock - @cantidad
                                  WHERE product_id = @id AND existencia_en_stock >= @cantidad";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@cantidad", cantidadVendida);
            command.Parameters.AddWithValue("@id", productId);

            return command.ExecuteNonQuery() > 0;
        }

        private void AgregarParametros(SqlCommand command, Producto p)
        {
            command.Parameters.AddWithValue("@nombre", p.NombreEtiqueta);
            command.Parameters.AddWithValue("@descripcion", p.Descripcion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@stock", p.ExistenciaEnStock);
            command.Parameters.AddWithValue("@limiteAlerta", p.ExistenciaLimiteParaAlerta);
            command.Parameters.AddWithValue("@precioNeto", p.PrecioNeto);
            command.Parameters.AddWithValue("@precioMinimo", p.PrecioMinimo);
            command.Parameters.AddWithValue("@precioMinimoConImp", p.PrecioMinimoConImpuesto);
            command.Parameters.AddWithValue("@tieneImpuesto", p.TieneImpuesto);
            command.Parameters.AddWithValue("@iva", p.ImpuestoValorAgregado);
            command.Parameters.AddWithValue("@impLocal1", p.ImpuestoLocal1);
            command.Parameters.AddWithValue("@impLocal2", p.ImpuestoLocal2);
            command.Parameters.AddWithValue("@puedeVenderse", p.PuedeVenderse);
            command.Parameters.AddWithValue("@puedeComprarse", p.PuedeComprarse);
            command.Parameters.AddWithValue("@codigoBarra", p.CodigoBarra ?? (object)DBNull.Value);
        }

        private Producto MapearProducto(SqlDataReader reader)
        {
            return new Producto
            {
                ProductId = reader.GetInt32(0),
                NombreEtiqueta = reader.GetString(1),
                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                ExistenciaEnStock = reader.GetInt32(3),
                ExistenciaLimiteParaAlerta = reader.GetInt32(4),
                PrecioNeto = reader.GetDecimal(5),
                PrecioMinimo = reader.GetDecimal(6),
                PrecioMinimoConImpuesto = reader.GetDecimal(7),
                TieneImpuesto = reader.GetBoolean(8),
                ImpuestoValorAgregado = reader.GetDecimal(9),
                ImpuestoLocal1 = reader.GetDecimal(10),
                ImpuestoLocal2 = reader.GetDecimal(11),
                PuedeVenderse = reader.GetBoolean(12),
                PuedeComprarse = reader.GetBoolean(13),
                CodigoBarra = reader.IsDBNull(14) ? null : reader.GetString(14)
            };
        }
    }
}