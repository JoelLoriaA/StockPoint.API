using System;
using System.Collections.Generic;
using System.Text;

namespace StockPoint.Domain
{
    using System.Collections.Generic;

    namespace StockPoint.Domain
    {
        public class Producto
        {
            public int ProductId { get; set; }
            public string NombreEtiqueta { get; set; }
            public string Descripcion { get; set; }
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
            public string CodigoBarra { get; set; }

            // Navegación opcional (categorías asociadas)
            public List<Categoria> Categorias { get; set; } = new List<Categoria>();
        }
    }
}
