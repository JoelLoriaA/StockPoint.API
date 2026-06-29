using StockPoint.Domain;

namespace StockPoint.Data.Interfaces
{
    public interface IOrdenData
    {
        int ProcesarOrden(PosOrden orden);
        PosOrden ObtenerPorNumero(int numeroOrden);
    }
}