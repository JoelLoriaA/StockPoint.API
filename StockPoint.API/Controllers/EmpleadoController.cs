using Microsoft.AspNetCore.Mvc;
using StockPoint.Business.Excepciones;
using StockPoint.Business.Interfaces;
using StockPoint.Domain;

namespace StockPoint.API.Controllers
{
    [ApiController]
    [Route("api/empleados")]
    public class EmpleadoController : ControllerBase
    {
        private readonly IEmpleadoBusiness _empleadoBusiness;

        public EmpleadoController(IEmpleadoBusiness empleadoBusiness)
        {
            _empleadoBusiness = empleadoBusiness;
        }

        // GET /api/empleados
        [HttpGet]
        public ActionResult<List<Empleado>> ObtenerTodos()
        {
            return Ok(_empleadoBusiness.ObtenerTodos());
        }

        // GET /api/empleados/{id}
        [HttpGet("{id:int}")]
        public ActionResult<Empleado> ObtenerPorId(int id)
        {
            try
            {
                var empleado = _empleadoBusiness.ObtenerPorId(id);
                if (empleado == null)
                    return NotFound(new { mensaje = $"No existe un empleado con id {id}." });

                return Ok(empleado);
            }
            catch (ValidacionException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
