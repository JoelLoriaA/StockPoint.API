using StockPoint.Domain;

namespace StockPoint.Business.Interfaces
{
    // Solo lectura: el Empleado no es nuestro actualizador, solo se consulta.
    public interface IEmpleadoBusiness
    {
        List<Empleado> ObtenerTodos();
        Empleado? ObtenerPorId(int id);
    }
}
