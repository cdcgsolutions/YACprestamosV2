using API1.Common;
using API1.DTOs.app;
using API1.RN.app;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers.app
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagoController : ControllerBase
    {
        private readonly PagoRN _pagoRN;

        public PagoController(PagoRN pagoRN)
        {
            _pagoRN = pagoRN;
        }

        [HttpPost("RegistrarAbono")]
        [ProducesResponseType(typeof(Result<PagoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<PagoDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<PagoDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarAbono([FromBody] RegistrarAbonoDTO dto) =>
            (await _pagoRN.RegistrarAbono(dto)).ToResult();

        [HttpGet("PorPrestamo/{idPrestamo}")]
        [ProducesResponseType(typeof(Result<ResumenHistorialPagosDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ResumenHistorialPagosDTO>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPagosPorPrestamo(int idPrestamo) =>
            (await _pagoRN.GetPagosPorPrestamo(idPrestamo)).ToResult();
    }
}

