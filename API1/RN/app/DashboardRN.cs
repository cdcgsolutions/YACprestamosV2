using API1.Common;
using API1.Context;
using API1.DTOs.app;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace API1.RN.app
{
    public class DashboardRN
    {
        private readonly YacPrestamosDbContext _context;

        public DashboardRN(YacPrestamosDbContext context)
        {
            _context = context;
        }

        public async Task<Result<DashboardResumenDTO>> ObtenerResumenAsync()
        {
            try
            {
                var totalClientes = await _context.Clientes.CountAsync();
                var clientesActivos = await _context.Clientes.CountAsync(c => c.EstaHabilitado);
                var totalPrestamos = await _context.Prestamos.CountAsync();
                var prestamosActivos = await _context.Prestamos.CountAsync(p => p.Estado == "Activo");
                var prestamosPagados = await _context.Prestamos.CountAsync(p => p.Estado == "Pagado");

                var totalCapital = await _context.Prestamos.SumAsync(p => (decimal?)p.Monto) ?? 0m;
                var totalCartera = await _context.Prestamos.SumAsync(p => (decimal?)p.MontoTotal) ?? 0m;
                var saldoPendiente = await _context.Prestamos.SumAsync(p => (decimal?)p.SaldoPendiente) ?? 0m;
                var totalRecaudado = await _context.Pagos.SumAsync(p => (decimal?)p.MontoAbonado) ?? 0m;

                var resumen = new DashboardResumenDTO
                {
                    TotalClientes = totalClientes,
                    ClientesActivos = clientesActivos,
                    TotalPrestamos = totalPrestamos,
                    PrestamosActivos = prestamosActivos,
                    PrestamosPagados = prestamosPagados,
                    TotalCapitalPrestado = totalCapital,
                    TotalMontoCartera = totalCartera,
                    TotalInteresGenerado = totalCartera - totalCapital,
                    SaldoPendienteTotal = saldoPendiente,
                    TotalRecaudado = totalRecaudado
                };

                resumen.DistribucionModalidades = await _context.Prestamos
                    .GroupBy(p => p.ModalidadCobro)
                    .Select(g => new ModalidadEstadisticaDTO
                    {
                        Modalidad = g.Key,
                        Cantidad = g.Count(),
                        MontoTotal = g.Sum(x => x.MontoTotal)
                    }).ToListAsync();

                return Result.Ok(resumen);
            }
            catch (Exception ex)
            {
                return Result.Fail500<DashboardResumenDTO>($"Error al calcular métricas: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> GenerarReporteExcelAsync(string titulo = "Reporte General de Cartera de Préstamos")
        {
            try
            {
                var prestamos = await _context.Prestamos
                    .Include(p => p.Cliente)
                    .Include(p => p.Pagos)
                    .OrderByDescending(p => p.FechaRegistro)
                    .ToListAsync();

                using (var libro = new XLWorkbook())
                {
                    var hoja = libro.Worksheets.Add("Cartera de Préstamos");

                    // Título Superior
                    var rangoTitulo = hoja.Range("A1:H1");
                    rangoTitulo.Merge().Value = titulo.ToUpper();
                    rangoTitulo.Style.Font.SetBold().Font.SetFontSize(15).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    rangoTitulo.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                    // Cabeceras de Columnas estilo SIS.INV
                    string[] columnas = { "ID", "CLIENTE", "MONTO CAPITAL (BS)", "% INT.", "TOTAL DEUDA (BS)", "TOTAL ABONADO (BS)", "SALDO PENDIENTE (BS)", "ESTADO" };
                    for (int i = 0; i < columnas.Length; i++)
                    {
                        var celda = hoja.Cell(3, i + 1);
                        celda.Value = columnas[i];
                        celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
                        celda.Style.Font.FontColor = XLColor.White;
                        celda.Style.Font.Bold = true;
                        celda.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    }

                    int fila = 4;
                    foreach (var p in prestamos)
                    {
                        hoja.Cell(fila, 1).Value = p.IdPrestamo;
                        hoja.Cell(fila, 2).Value = p.Cliente?.Nombre ?? "N/A";
                        hoja.Cell(fila, 3).Value = p.Monto;
                        hoja.Cell(fila, 4).Value = $"{p.PorcentajeInteres}%";
                        hoja.Cell(fila, 5).Value = p.MontoTotal;
                        hoja.Cell(fila, 6).Value = p.Pagos.Sum(x => x.MontoAbonado);
                        hoja.Cell(fila, 7).Value = p.SaldoPendiente;

                        var celdaEstado = hoja.Cell(fila, 8);
                        celdaEstado.Value = p.Estado.ToUpper();
                        celdaEstado.Style.Font.Bold = true;
                        celdaEstado.Style.Font.FontColor = p.Estado == "Pagado" ? XLColor.Green : XLColor.FromHtml("#d35400");

                        fila++;
                    }

                    hoja.Columns().AdjustToContents();
                    var tabla = hoja.Range(3, 1, fila - 1, 8);
                    tabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tabla.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    using (var memoria = new MemoryStream())
                    {
                        libro.SaveAs(memoria);
                        return Result.Ok(memoria.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Fail500<byte[]>($"Error al generar reporte Excel: {ex.Message}");
            }
        }
    }
}

