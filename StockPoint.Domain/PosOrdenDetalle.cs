using System;
using System.Collections.Generic;
using System.Text;



    namespace StockPoint.Domain
    {
        public class PosOrdenDetalle
        {
            public int FkPosOrden { get; set; }
            public int FkProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal Impuesto { get; set; }
            public decimal Descuento { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal PrecioSubtotal { get; set; }

            
            public string NombreProducto { get; set; }
            public string CodigoProducto { get; set; }
        }
    }
