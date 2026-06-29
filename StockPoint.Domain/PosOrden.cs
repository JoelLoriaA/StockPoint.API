using System;
using System.Collections.Generic;
using System.Text;

namespace StockPoint.Domain
{
    using System;
    using System.Collections.Generic;

    namespace StockPoint.Domain
    {
        public class PosOrden
        {
            public int NumeroOrden { get; set; }
            public int FkCliente { get; set; }
            public int FkEmpleado { get; set; }
            public DateTime FechaOrden { get; set; }
            public decimal TotalOrden { get; set; }
            public decimal Impuesto { get; set; }

            // Detalle de la orden (maestro-detalle)
            public List<PosOrdenDetalle> Detalles { get; set; } = new List<PosOrdenDetalle>();

            // Navegación opcional, útil para mostrar info del cliente en la matriz del WPF
            public string NombreCliente { get; set; }
        }
    }
}
