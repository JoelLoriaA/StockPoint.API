using Microsoft.AspNetCore.Mvc;
using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Domain;

namespace StockPoint.API.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteBusiness _clienteBusiness;

        public ClienteController(IClienteBusiness clienteBusiness)
        {
            _clienteBusiness = clienteBusiness;
        }

        // GET /api/clientes
        [HttpGet]
        public ActionResult<List<Cliente>> ObtenerTodos()
        {
            return Ok(_clienteBusiness.ObtenerTodos());
        }

        // GET /api/clientes/{id}
        [HttpGet("{id:int}")]
        public ActionResult<Cliente> ObtenerPorId(int id)
        {
            try
            {
                var cliente = _clienteBusiness.ObtenerPorId(id);
                if (cliente == null)
                    return NotFound(new { mensaje = $"No existe un cliente con id {id}." });

                return Ok(cliente);
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
