using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Data.Interfaces;
using StockPoint.Domain;

namespace StockPoint.Business
{
    public class ClienteBusiness : IClienteBusiness
    {
        private readonly IClienteData _clienteData;

        public ClienteBusiness(IClienteData clienteData)
        {
            _clienteData = clienteData;
        }

        public List<Cliente> ObtenerTodos()
        {
            return _clienteData.ObtenerTodos();
        }

        public Cliente? ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ValidacionException("El identificador del cliente debe ser mayor que cero.");

            return _clienteData.ObtenerPorId(id);
        }
    }
}
