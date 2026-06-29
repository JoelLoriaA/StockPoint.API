using StockPoint.Domain;

namespace StockPoint.Data.Interfaces
{
    public interface IClienteData
    {
        List<Cliente> ObtenerTodos();
        Cliente ObtenerPorId(int id);
    }
}
