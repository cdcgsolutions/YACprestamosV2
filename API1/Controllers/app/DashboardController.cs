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
        /// Obtiene métricas agregadas y estadísticas de la cartera de préstamos para el dashboard.
        /// </summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen() =>
            (await _dashboardRN.ObtenerResumenAsync()).ToResult();

        /// <summary>
        /// Genera y descarga un reporte ejecutivo completo en Excel con ClosedXML.
        /// </summary>
        [HttpGet("reporte-excel")]
        public async Task<IActionResult> DescargarReporteExcel([FromQuery] string titulo = "Reporte General de Cartera de Préstamos") =>
            (await _dashboardRN.GenerarReporteExcelAsync(titulo)).ToResult();
    }
}

