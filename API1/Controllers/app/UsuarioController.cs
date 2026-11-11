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

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsuarioById(int id) =>
            (await _usuarioRN.GetUsuarioById(id)).ToResult();

        [HttpPost]
        [HttpPost("Creacion")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostUsuario([FromBody] UsuarioCreacionDTO dto) =>
            (await _usuarioRN.PostUsuario(dto)).ToResult();

        [HttpPut]
        [HttpPut("Edicion")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutUsuario([FromBody] UsuarioEdicionDTO dto) =>
            (await _usuarioRN.PutUsuario(dto)).ToResult();

        [HttpPut("CambiarEstado/{id}")]
        [HttpPatch("{id}/cambiar-estado")]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UsuarioDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id) =>
            (await _usuarioRN.CambiarEstado(id)).ToResult();
    }
}

