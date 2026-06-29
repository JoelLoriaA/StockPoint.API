using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Data.Interfaces;
using StockPoint.Domain;

namespace StockPoint.Business
{
    public class EmpleadoBusiness : IEmpleadoBusiness
    {
        private readonly IEmpleadoData _empleadoData;

        public EmpleadoBusiness(IEmpleadoData empleadoData)
        {
            _empleadoData = empleadoData;
        }

        public List<Empleado> ObtenerTodos()
        {
            return _empleadoData.ObtenerTodos();
        }

        public Empleado? ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ValidacionException("El identificador del empleado debe ser mayor que cero.");

            return _empleadoData.ObtenerPorId(id);
        }
    }
}
