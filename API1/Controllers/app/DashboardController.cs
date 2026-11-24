using API1.Common;
using API1.RN.app;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers.app
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardRN _dashboardRN;

        public DashboardController(DashboardRN dashboardRN)
        {
            _dashboardRN = dashboardRN;
        }

        /// <summary>
        /// Obtiene métricas agregadas y estadísticas de la cartera de préstamos para el dashboard (con filtros multi-criterio).
        /// </summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen(
            [FromQuery] int? idInstitucion = null,
            [FromQuery] string? modalidad = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null) =>
            (await _dashboardRN.ObtenerResumenAsync(idInstitucion, modalidad, estado, mes, ano, fechaInicio, fechaFin)).ToResult();

        /// <summary>
        /// Genera y descarga un reporte ejecutivo completo en Excel con ClosedXML (con los mismos filtros multi-criterio).
        /// </summary>
        [HttpGet("reporte-excel")]
        public async Task<IActionResult> DescargarReporteExcel(
            [FromQuery] int? idInstitucion = null,
            [FromQuery] string? modalidad = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string titulo = "Reporte Ejecutivo de Cartera de Préstamos") =>
            (await _dashboardRN.GenerarReporteExcelAsync(idInstitucion, modalidad, estado, mes, ano, fechaInicio, fechaFin, titulo)).ToResult();
    }
}
