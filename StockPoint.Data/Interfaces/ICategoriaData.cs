using Microsoft.Data.SqlClient;
using StockPoint.Domain;

namespace StockPoint.Data.Interfaces
{
    public interface ICategoriaData
    {
        List<Categoria> ObtenerTodas();
        Categoria ObtenerPorId(int id);
        List<Categoria> ObtenerPorProducto(int productId, SqlConnection connection);
        void AsignarCategorias(int productId, List<int> idsCategoria, SqlConnection connection, SqlTransaction transaction = null);
        void EliminarCategoriasDeProducto(int productId, SqlConnection connection, SqlTransaction transaction = null);
    }
}
