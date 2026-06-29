using Microsoft.AspNetCore.Mvc;
using StockPoint.API.DTOs;
using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Domain;

namespace StockPoint.API.Controllers
{
    [ApiController]
    [Route("api/ordenes")]
    public class OrdenController : ControllerBase
    {
        private readonly IOrdenBusiness _ordenBusiness;

        public OrdenController(IOrdenBusiness ordenBusiness)
        {
            _ordenBusiness = ordenBusiness;
        }

        // POST /api/ordenes
        // Recibe un DTO con solo los campos que aporta el cliente; los campos
        // calculados (NombreCliente, NombreProducto, totales) los rellena Business.
        [HttpPost]
        public ActionResult<PosOrden> Crear([FromBody] CrearOrdenRequest request)
        {
            try
            {
                var orden = new PosOrden
                {
                    FkCliente  = request.FkCliente,
                    FkEmpleado = request.FkEmpleado,
                    Detalles   = request.Detalles.Select(d => new PosOrdenDetalle
                    {
                        FkProducto = d.FkProducto,
                        Cantidad   = d.Cantidad,
                        Descuento  = d.Descuento
                    }).ToList()
                };

                int numeroOrden = _ordenBusiness.CrearOrden(orden);

                // Se devuelve la orden ya persistida (con totales calculados) y 201 Created.
                var creada = _ordenBusiness.ObtenerOrden(numeroOrden);
                return CreatedAtAction(nameof(ObtenerPorNumero), new { numero = numeroOrden }, creada);
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (EntidadNoEncontradaException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (StockInsuficienteException ex)
            {
                // 409 Conflict: la orden es válida pero no hay existencia suficiente.
                return Conflict(new { mensaje = ex.Message });
            }
        }

        // GET /api/ordenes/{numero}
        [HttpGet("{numero:int}")]
        public ActionResult<PosOrden> ObtenerPorNumero(int numero)
        {
            try
            {
                var orden = _ordenBusiness.ObtenerOrden(numero);
                if (orden == null)
                    return NotFound(new { mensaje = $"No existe una orden con número {numero}." });

                return Ok(orden);
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
