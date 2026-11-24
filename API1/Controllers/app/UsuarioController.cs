using API1.Common;
using API1.DTOs.app;
using API1.RN.app;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers.app
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioRN _usuarioRN;

        public UsuarioController(UsuarioRN usuarioRN)
        {
            _usuarioRN = usuarioRN;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Result<List<UsuarioDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsuarios() =>
            (await _usuarioRN.GetUsuarios()).ToResult();

        [HttpGet("PaginadoAndBuscador")]
        [ProducesResponseType(typeof(Result<ResponseListDTO<UsuarioDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsuariosPaginadoAndBuscador(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? q = null,
            [FromQuery] string? rol = null,
            [FromQuery] bool soloActivos = false,
            CancellationToken c = default) =>
            (await _usuarioRN.GetUsuariosPaginadoAndBuscador(pageNumber, pageSize, q, rol, soloActivos, c)).ToResult();

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsuarioById(int id) =>
            (await _usuarioRN.GetUsuarioById(id)).ToResult();

        [HttpPost("Creacion")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostUsuario([FromBody] UsuarioCreacionDTO dto) =>
            (await _usuarioRN.PostUsuario(dto)).ToResult();

        [HttpPut("Edicion")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutUsuario([FromBody] UsuarioEdicionDTO dto) =>
            (await _usuarioRN.PutUsuario(dto)).ToResult();

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id) =>
            (await _usuarioRN.CambiarEstado(id)).ToResult();
    }
}
