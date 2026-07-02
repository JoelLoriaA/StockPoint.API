using Microsoft.AspNetCore.Mvc;
using StockPoint.API.DTOs;
using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Domain;

namespace StockPoint.API.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoBusiness _productoBusiness;
        private readonly IOrdenBusiness _ordenBusiness;

        public ProductoController(IProductoBusiness productoBusiness, IOrdenBusiness ordenBusiness)
        {
            _productoBusiness = productoBusiness;
            _ordenBusiness = ordenBusiness;
        }

        // GET /api/productos
        [HttpGet]
        public ActionResult<List<Producto>> ObtenerTodos()
        {
            return Ok(_productoBusiness.ObtenerTodos());
        }

        // GET /api/productos/{id}
        [HttpGet("{id:int}")]
        public ActionResult<Producto> ObtenerPorId(int id)
        {
            try
            {
                var producto = _productoBusiness.ObtenerPorId(id);
                if (producto == null)
                    return NotFound(new { mensaje = $"No existe un producto con id {id}." });

                return Ok(producto);
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // GET /api/productos/buscar?termino=...
        [HttpGet("buscar")]
        public ActionResult<List<Producto>> Buscar([FromQuery] string termino)
        {
            try
            {
                return Ok(_productoBusiness.Buscar(termino));
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // GET /api/productos/{id}/ordenes
        // Órdenes en las que se vendió el producto (para la vista "Ver producto").
        [HttpGet("{id:int}/ordenes")]
        public ActionResult<List<OrdenPorProducto>> ObtenerOrdenes(int id)
        {
            try
            {
                return Ok(_ordenBusiness.ObtenerOrdenesPorProducto(id));
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (EntidadNoEncontradaException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        // POST /api/productos
        // Recibe un DTO; el cliente envía IdsCategoria (lista de ints) en lugar de
        // objetos Categoria completos, ya que NombreCategoria lo resuelve la BD.
        [HttpPost]
        public ActionResult<Producto> Crear([FromBody] ProductoRequest request)
        {
            try
            {
                var producto = MapearDesdeRequest(request);
                int nuevoId = _productoBusiness.Crear(producto);

                var creado = _productoBusiness.ObtenerPorId(nuevoId);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, creado);
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PUT /api/productos/{id}
        [HttpPut("{id:int}")]
        public IActionResult Actualizar(int id, [FromBody] ProductoRequest request)
        {
            try
            {
                var producto = MapearDesdeRequest(request);
                _productoBusiness.Actualizar(id, producto);
                return NoContent();
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (EntidadNoEncontradaException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        // DELETE /api/productos/{id}
        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int id)
        {
            try
            {
                _productoBusiness.Eliminar(id);
                return NoContent();
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (EntidadNoEncontradaException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        // Convierte ProductoRequest → Producto (Domain).
        // NombreCategoria se deja vacío; la BD lo rellena en la lectura posterior.
        private static Producto MapearDesdeRequest(ProductoRequest r) => new()
        {
            NombreEtiqueta             = r.NombreEtiqueta,
            Descripcion                = r.Descripcion,
            ExistenciaEnStock          = r.ExistenciaEnStock,
            ExistenciaLimiteParaAlerta = r.ExistenciaLimiteParaAlerta,
            PrecioNeto                 = r.PrecioNeto,
            PrecioMinimo               = r.PrecioMinimo,
            PrecioMinimoConImpuesto    = r.PrecioMinimoConImpuesto,
            TieneImpuesto              = r.TieneImpuesto,
            ImpuestoValorAgregado      = r.ImpuestoValorAgregado,
            ImpuestoLocal1             = r.ImpuestoLocal1,
            ImpuestoLocal2             = r.ImpuestoLocal2,
            PuedeVenderse              = r.PuedeVenderse,
            PuedeComprarse             = r.PuedeComprarse,
            CodigoBarra                = r.CodigoBarra,
            Categorias                 = r.IdsCategoria
                                          .Select(id => new Categoria { IdCategoria = id, NombreCategoria = "" })
                                          .ToList()
        };
    }
}
