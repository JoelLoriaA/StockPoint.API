using System;

namespace StockPoint.Domain
{
    // Resumen de una orden en la que aparece un producto dado.
    // Pensado para la vista "Ver producto": muestra en qué órdenes se vendió
    // el producto y con qué número consultarlas después.
    public class OrdenPorProducto
    {
        public int NumeroOrden { get; set; }
        public DateTime FechaOrden { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
