using StockPoint.Domain;

namespace StockPoint.Business.Interfaces
{
    // Solo lectura: el Cliente no es nuestro actualizador, solo se consulta.
    public interface IClienteBusiness
    {
        List<Cliente> ObtenerTodos();
        Cliente? ObtenerPorId(int id);
    }
}
