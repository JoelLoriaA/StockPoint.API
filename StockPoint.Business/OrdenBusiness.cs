using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Data.Interfaces;
using StockPoint.Domain;

namespace StockPoint.Business
{
    public class OrdenBusiness : IOrdenBusiness
    {
        private readonly IOrdenData _ordenData;
        private readonly IClienteBusiness _clienteBusiness;
        private readonly IEmpleadoBusiness _empleadoBusiness;
        private readonly IProductoBusiness _productoBusiness;

        public OrdenBusiness(
            IOrdenData ordenData,
            IClienteBusiness clienteBusiness,
            IEmpleadoBusiness empleadoBusiness,
            IProductoBusiness productoBusiness)
        {
            _ordenData = ordenData;
            _clienteBusiness = clienteBusiness;
            _empleadoBusiness = empleadoBusiness;
            _productoBusiness = productoBusiness;
        }

        public int CrearOrden(PosOrden orden)
        {
            // ---- 1. Validaciones de estructura ----
            if (orden == null)
                throw new ValidacionException("La orden no puede ser nula.");

            if (orden.Detalles == null || orden.Detalles.Count == 0)
                throw new ValidacionException("La orden debe tener al menos una línea de detalle.");

            // ---- 2. Validar existencia de cliente y empleado (vía sus Business) ----
            if (_clienteBusiness.ObtenerPorId(orden.FkCliente) == null)
                throw new EntidadNoEncontradaException($"No existe un cliente con id {orden.FkCliente}.");

            if (_empleadoBusiness.ObtenerPorId(orden.FkEmpleado) == null)
                throw new EntidadNoEncontradaException($"No existe un empleado con id {orden.FkEmpleado}.");

            // ---- 3. Calcular cada línea y acumular totales ----
            decimal impuestoTotal = 0m;
            decimal subtotalGeneral = 0m;

            foreach (var detalle in orden.Detalles)
            {
                if (detalle.Cantidad <= 0)
                    throw new ValidacionException("La cantidad de cada línea debe ser mayor que cero.");

                if (detalle.Descuento < 0)
                    throw new ValidacionException("El descuento de una línea no puede ser negativo.");

                var producto = _productoBusiness.ObtenerPorId(detalle.FkProducto);
                if (producto == null)
                    throw new EntidadNoEncontradaException($"No existe un producto con id {detalle.FkProducto}.");

                if (!producto.PuedeVenderse)
                    throw new ValidacionException(
                        $"El producto '{producto.NombreEtiqueta}' (id {producto.ProductId}) no está disponible para la venta.");

                if (detalle.Cantidad > producto.ExistenciaEnStock)
                    throw new StockInsuficienteException(
                        $"No hay existencia suficiente para el producto '{producto.NombreEtiqueta}' " +
                        $"(disponible: {producto.ExistenciaEnStock}, solicitado: {detalle.Cantidad}).");

                // El precio unitario es el del catálogo, no el que envíe el cliente (integridad).
                detalle.PrecioUnitario = producto.PrecioNeto;

                // Subtotal de la línea = precio_unitario * cantidad - descuento
                decimal subtotalLinea = (detalle.PrecioUnitario * detalle.Cantidad) - detalle.Descuento;
                if (subtotalLinea < 0)
                    throw new ValidacionException(
                        $"El descuento de la línea del producto {producto.ProductId} supera el monto de la línea.");

                detalle.PrecioSubtotal = Redondear(subtotalLinea);

                // Impuesto de la línea: solo si el producto tiene impuesto (IVA sobre el subtotal).
                decimal impuestoLinea = 0m;
                if (producto.TieneImpuesto)
                    impuestoLinea = Redondear(NormalizarTasa(producto.ImpuestoValorAgregado) * detalle.PrecioSubtotal);

                detalle.Impuesto = impuestoLinea;

                // Datos de apoyo para mostrar la orden sin otro llamado.
                detalle.NombreProducto = producto.NombreEtiqueta;
                detalle.CodigoProducto = producto.CodigoBarra;

                subtotalGeneral += detalle.PrecioSubtotal;
                impuestoTotal += impuestoLinea;
            }

            // ---- 4. Totales de la cabecera ----
            orden.Impuesto = Redondear(impuestoTotal);
            orden.TotalOrden = Redondear(subtotalGeneral + impuestoTotal);

            if (orden.FechaOrden == default)
                orden.FechaOrden = DateTime.Now;

            // ---- 5. Procesar transaccionalmente en la capa de datos ----
            try
            {
                return _ordenData.ProcesarOrden(orden);
            }
            catch (InvalidOperationException ex)
            {
                // La capa de datos lanza InvalidOperationException cuando no hay
                // existencia suficiente. Se traduce a una excepción de negocio propia.
                throw new StockInsuficienteException(ex.Message, ex);
            }
        }

        public PosOrden? ObtenerOrden(int numero)
        {
            if (numero <= 0)
                throw new ValidacionException("El número de orden debe ser mayor que cero.");

            return _ordenData.ObtenerPorNumero(numero);
        }

        public List<OrdenPorProducto> ObtenerOrdenesPorProducto(int productId)
        {
            if (productId <= 0)
                throw new ValidacionException("El identificador del producto debe ser mayor que cero.");

            // Se valida la existencia del producto para poder devolver 404 si no existe
            // (una lista vacía significa "existe pero no tiene órdenes").
            if (_productoBusiness.ObtenerPorId(productId) == null)
                throw new EntidadNoEncontradaException($"No existe un producto con id {productId}.");

            return _ordenData.ObtenerPorProducto(productId);
        }

        /// <summary>
        /// Acepta el IVA tanto como fracción (0.13) como porcentaje (13) y lo
        /// devuelve siempre como fracción para multiplicar por el subtotal.
        /// </summary>
        private static decimal NormalizarTasa(decimal tasa)
        {
            return tasa > 1m ? tasa / 100m : tasa;
        }

        private static decimal Redondear(decimal valor)
        {
            return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        }
    }
}
