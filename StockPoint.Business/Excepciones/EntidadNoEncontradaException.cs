namespace StockPoint.Business.Excepciones
{
    /// <summary>
    /// Se lanza cuando se referencia una entidad que no existe en la base de datos
    /// (un cliente, empleado o producto inexistente dentro de una orden).
    /// El controlador la traduce a un HTTP 404 (Not Found).
    /// </summary>
    public class EntidadNoEncontradaException : Exception
    {
        public EntidadNoEncontradaException(string mensaje) : base(mensaje) { }
    }
}
