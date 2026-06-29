using StockPoint.Domain;

namespace StockPoint.Business.Interfaces
{
    public interface IProductoBusiness
    {
        List<Producto> ObtenerTodos();
        Producto? ObtenerPorId(int id);
        List<Producto> Buscar(string termino);
        int Crear(Producto producto);
        bool Actualizar(int id, Producto producto);
        bool Eliminar(int id);
    }
}
