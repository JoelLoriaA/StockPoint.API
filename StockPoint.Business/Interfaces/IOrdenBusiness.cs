using StockPoint.Domain;

namespace StockPoint.Business.Interfaces
{
    public interface IOrdenBusiness
    {
        /// <summary>
        /// Valida la orden, calcula subtotales/impuestos/total de cada línea y de la
        /// cabecera, y la envía a la capa de datos para procesarla transaccionalmente.
        /// Devuelve el número de orden generado.
        /// </summary>
        int CrearOrden(PosOrden orden);

        PosOrden? ObtenerOrden(int numero);
    }
}
