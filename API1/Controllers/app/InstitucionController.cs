using API1.Common;
using API1.DTOs.app;
using API1.RN.app;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers.app
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstitucionController : ControllerBase
    {
        private readonly InstitucionRN _institucionRN;

        public InstitucionController(InstitucionRN institucionRN)
        {
            _institucionRN = institucionRN;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Result<List<InstitucionDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInstituciones([FromQuery] bool soloActivos = false) =>
            (await _institucionRN.GetInstituciones(soloActivos)).ToResult();

        [HttpGet("PaginadoAndBuscador")]
        [ProducesResponseType(typeof(Result<ResponseListDTO<InstitucionDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInstitucionesPaginadoAndBuscador(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? q = null,
            [FromQuery] bool soloActivos = false,
            CancellationToken c = default) =>
            (await _institucionRN.GetInstitucionesPaginadoAndBuscador(pageNumber, pageSize, q, soloActivos, c)).ToResult();

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInstitucionById(int id) =>
            (await _institucionRN.GetInstitucionById(id)).ToResult();

        [HttpPost("Creacion")]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostInstitucion([FromBody] InstitucionDTO dto) =>
            (await _institucionRN.PostInstitucion(dto)).ToResult();

        [HttpPut("Edicion")]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutInstitucion([FromBody] InstitucionDTO dto) =>
            (await _institucionRN.PutInstitucion(dto)).ToResult();

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<InstitucionDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id) =>
            (await _institucionRN.CambiarEstado(id)).ToResult();
    }
}
