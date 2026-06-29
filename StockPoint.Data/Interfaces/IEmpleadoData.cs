using StockPoint.Domain;

namespace StockPoint.Data.Interfaces
{
    public interface IEmpleadoData
    {
        List<Empleado> ObtenerTodos();
        Empleado ObtenerPorId(int id);
    }
}
