using Microsoft.Data.SqlClient;
using StockPoint.Domain;
using StockPoint.Data.Interfaces;

namespace StockPoint.Data
{
    public class CategoriaData : ICategoriaData
    {
        private readonly ConexionBD _conexion;

        public CategoriaData(ConexionBD conexion)
        {
            _conexion = conexion;
        }

        public List<Categoria> ObtenerTodas()
        {
            var lista = new List<Categoria>();
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = "SELECT id_categoria, nombre_categoria FROM Categoria";

            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Categoria
                {
                    IdCategoria = reader.GetInt32(0),
                    NombreCategoria = reader.GetString(1)
                });
            }
            return lista;
        }

        public Categoria ObtenerPorId(int id)
        {
            using var connection = _conexion.ObtenerConexion();
            connection.Open();

            const string sql = "SELECT id_categoria, nombre_categoria FROM Categoria WHERE id_categoria = @id";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Categoria { IdCategoria = reader.GetInt32(0), NombreCategoria = reader.GetString(1) };
            }
            return null;
        }

        // Usado por ProductoData, reutiliza la conexión que ya está abierta (no abre una nueva)
        public List<Categoria> ObtenerPorProducto(int productId, SqlConnection connection)
        {
            var lista = new List<Categoria>();

            const string sql = @"SELECT c.id_categoria, c.nombre_categoria
                                  FROM Categoria c
                                  INNER JOIN Producto_Categoria pc ON pc.fk_categoria = c.id_categoria
                                  WHERE pc.fk_producto = @productId";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@productId", productId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Categoria { IdCategoria = reader.GetInt32(0), NombreCategoria = reader.GetString(1) });
            }
            return lista;
        }

        // Inserta las relaciones Producto_Categoria para un producto. Usado en Crear/Actualizar de ProductoData.
        public void AsignarCategorias(int productId, List<int> idsCategoria, SqlConnection connection, SqlTransaction transaction = null)
        {
            if (idsCategoria == null) return;

            foreach (var idCategoria in idsCategoria)
            {
                const string sql = @"INSERT INTO Producto_Categoria (fk_producto, fk_categoria)
                                      VALUES (@productId, @categoriaId)";

                using var command = transaction != null
                    ? new SqlCommand(sql, connection, transaction)
                    : new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@productId", productId);
                command.Parameters.AddWithValue("@categoriaId", idCategoria);
                command.ExecuteNonQuery();
            }
        }

        // Limpia las relaciones antes de reinsertar (usado en Actualizar y en Eliminar de ProductoData)
        public void EliminarCategoriasDeProducto(int productId, SqlConnection connection, SqlTransaction transaction = null)
        {
            const string sql = "DELETE FROM Producto_Categoria WHERE fk_producto = @productId";

            using var command = transaction != null
                ? new SqlCommand(sql, connection, transaction)
                : new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@productId", productId);
            command.ExecuteNonQuery();
        }
    }
}
