using Microsoft.Data.SqlClient;
using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Data.Interfaces;
using StockPoint.Domain;

namespace StockPoint.Business
{
    public class ProductoBusiness : IProductoBusiness
    {
        private readonly IProductoData _productoData;

        public ProductoBusiness(IProductoData productoData)
        {
            _productoData = productoData;
        }

        public List<Producto> ObtenerTodos()
        {
            return _productoData.ObtenerTodos();
        }

        public Producto? ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ValidacionException("El identificador del producto debe ser mayor que cero.");

            return _productoData.ObtenerPorId(id);
        }

        public List<Producto> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                throw new ValidacionException("Debe indicar un término de búsqueda.");

            return _productoData.BuscarPorNombreOCodigo(termino.Trim());
        }

        public int Crear(Producto producto)
        {
            Validar(producto);
            return _productoData.Crear(producto);
        }

        public bool Actualizar(int id, Producto producto)
        {
            if (id <= 0)
                throw new ValidacionException("El identificador del producto debe ser mayor que cero.");

            // El id de la ruta es la fuente de verdad, no el del cuerpo.
            producto.ProductId = id;
            Validar(producto);

            if (_productoData.ObtenerPorId(id) == null)
                throw new EntidadNoEncontradaException($"No existe un producto con id {id}.");

            return _productoData.Actualizar(producto);
        }

        public bool Eliminar(int id)
        {
            if (id <= 0)
                throw new ValidacionException("El identificador del producto debe ser mayor que cero.");

            if (_productoData.ObtenerPorId(id) == null)
                throw new EntidadNoEncontradaException($"No existe un producto con id {id}.");

            try
            {
                return _productoData.Eliminar(id);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Error 547: violación de FK — el producto tiene órdenes asociadas.
                throw new ValidacionException(
                    "No se puede eliminar el producto porque tiene órdenes asociadas.");
            }
        }

        /// <summary>
        /// Reglas de negocio para crear/actualizar un producto.
        /// </summary>
        private static void Validar(Producto producto)
        {
            if (producto == null)
                throw new ValidacionException("El producto no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(producto.NombreEtiqueta))
                throw new ValidacionException("El nombre (etiqueta) del producto es obligatorio.");

            if (producto.PrecioNeto <= 0)
                throw new ValidacionException("El precio neto debe ser mayor que cero.");

            if (producto.PrecioMinimo < 0)
                throw new ValidacionException("El precio mínimo no puede ser negativo.");

            if (producto.ExistenciaEnStock < 0)
                throw new ValidacionException("La existencia en stock no puede ser negativa.");

            if (producto.ExistenciaLimiteParaAlerta < 0)
                throw new ValidacionException("El límite para alerta no puede ser negativo.");

            if (producto.TieneImpuesto && producto.ImpuestoValorAgregado < 0)
                throw new ValidacionException("El impuesto al valor agregado no puede ser negativo.");
        }
    }
}
