using API1.Common;
using API1.DTOs.app;
using API1.RN.app;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers.app
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly ClienteRN _clienteRN;

        public ClienteController(ClienteRN clienteRN)
        {
            _clienteRN = clienteRN;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Result<List<ClienteDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientes([FromQuery] bool soloActivos = false) =>
            (await _clienteRN.GetClientes(soloActivos)).ToResult();

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClienteById(int id) =>
            (await _clienteRN.GetClienteById(id)).ToResult();

        [HttpPost]
        [HttpPost("Creacion")]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostCliente([FromBody] ClienteDTO dto) =>
            (await _clienteRN.PostCliente(dto)).ToResult();

        [HttpPut]
        [HttpPut("Edicion")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCliente([FromBody] ClienteDTO dto) =>
            (await _clienteRN.PutCliente(dto)).ToResult();

        [HttpPut("CambiarEstado/{id}")]
        [HttpPatch("{id}/cambiar-estado")]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ClienteDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CambiarEstado(int id) =>
            (await _clienteRN.CambiarEstado(id)).ToResult();
    }
}

