using Microsoft.Data.SqlClient;
using StockPoint.Domain;

namespace StockPoint.Data.Interfaces
{
    public interface IProductoData
    {
        List<Producto> ObtenerTodos();
        Producto ObtenerPorId(int id);
        List<Producto> BuscarPorNombreOCodigo(string termino);
        int Crear(Producto producto);
        bool Actualizar(Producto producto);
        bool Eliminar(int id);
        bool ActualizarStock(int productId, int cantidadVendida, SqlTransaction transaction, SqlConnection connection);
    }
}
