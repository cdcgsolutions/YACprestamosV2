using API1.Common;
using API1.DTOs.app;
using API1.RN.app;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers.app
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamoController : ControllerBase
    {
        private readonly PrestamoRN _prestamoRN;

        public PrestamoController(PrestamoRN prestamoRN)
        {
            _prestamoRN = prestamoRN;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Result<List<PrestamoDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrestamos([FromQuery] bool soloActivos = false) =>
            (await _prestamoRN.GetPrestamos(soloActivos)).ToResult();

        [HttpGet("PaginadoAndBuscador")]
        [ProducesResponseType(typeof(Result<ResponseListDTO<PrestamoDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrestamosPaginadoAndBuscador(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? q = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? idCliente = null,
            CancellationToken c = default) =>
            (await _prestamoRN.GetPrestamosPaginadoAndBuscador(pageNumber, pageSize, q, estado, idCliente, c)).ToResult();

        [HttpGet("PorCliente/{idCliente}")]
        [ProducesResponseType(typeof(Result<List<PrestamoDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrestamosPorCliente(int idCliente) =>
            (await _prestamoRN.GetPrestamosPorCliente(idCliente)).ToResult();

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<PrestamoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PrestamoDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPrestamoById(int id) =>
            (await _prestamoRN.GetPrestamoById(id)).ToResult();

        [HttpPost("Creacion")]
        [ProducesResponseType(typeof(Result<PrestamoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PrestamoDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostPrestamo([FromBody] PrestamoCreacionDTO dto) =>
            (await _prestamoRN.PostPrestamo(dto)).ToResult();

        [HttpPut("Edicion")]
        [ProducesResponseType(typeof(Result<PrestamoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PrestamoDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<PrestamoDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPrestamo([FromBody] PrestamoDTO dto) =>
            (await _prestamoRN.PutPrestamo(dto)).ToResult();
    }
}
