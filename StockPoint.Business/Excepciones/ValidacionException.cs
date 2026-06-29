namespace StockPoint.Business.Excepciones
{
    /// <summary>
    /// Se lanza cuando los datos que llegan a la capa de negocios no cumplen
    /// una regla de validación (campos obligatorios, precios inválidos, etc.).
    /// El controlador la traduce a un HTTP 400 (Bad Request).
    /// </summary>
    public class ValidacionException : Exception
    {
        public ValidacionException(string mensaje) : base(mensaje) { }
    }
}
