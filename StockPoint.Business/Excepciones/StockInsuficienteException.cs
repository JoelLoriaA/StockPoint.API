namespace StockPoint.Business.Excepciones
{
    /// <summary>
    /// Se lanza cuando, al procesar una orden, no hay existencia suficiente de
    /// algún producto. Es la traducción de negocio de la InvalidOperationException
    /// que lanza la capa de datos (OrdenData.ProcesarOrden).
    /// El controlador la traduce a un HTTP 409 (Conflict).
    /// </summary>
    public class StockInsuficienteException : Exception
    {
        public StockInsuficienteException(string mensaje) : base(mensaje) { }

        public StockInsuficienteException(string mensaje, Exception inner) : base(mensaje, inner) { }
    }
}
