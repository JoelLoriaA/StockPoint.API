namespace StockPoint.API.DTOs
{
    // DTO para crear o actualizar un producto.
    // El cliente envía los IDs de las categorías; el nombre de cada categoría
    // lo resuelve la capa de datos cuando devuelve el producto persistido.
    public class ProductoRequest
    {
        public string NombreEtiqueta { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int ExistenciaEnStock { get; set; }
        public int ExistenciaLimiteParaAlerta { get; set; }
        public decimal PrecioNeto { get; set; }
        public decimal PrecioMinimo { get; set; }
        public decimal PrecioMinimoConImpuesto { get; set; }
        public bool TieneImpuesto { get; set; }
        public decimal ImpuestoValorAgregado { get; set; }
        public decimal ImpuestoLocal1 { get; set; }
        public decimal ImpuestoLocal2 { get; set; }
        public bool PuedeVenderse { get; set; }
        public bool PuedeComprarse { get; set; }
        public string? CodigoBarra { get; set; }
        public List<int> IdsCategoria { get; set; } = new();
    }
}
