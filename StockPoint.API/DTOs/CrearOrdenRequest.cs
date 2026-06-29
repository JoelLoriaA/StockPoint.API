namespace StockPoint.API.DTOs
{
    public class CrearOrdenRequest
    {
        public int FkCliente { get; set; }
        public int FkEmpleado { get; set; }
        public List<CrearDetalleRequest> Detalles { get; set; } = new();
    }

    public class CrearDetalleRequest
    {
        public int FkProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Descuento { get; set; }
    }
}
