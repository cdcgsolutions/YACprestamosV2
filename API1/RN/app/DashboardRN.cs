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

        public async Task<Result<DashboardResumenDTO>> ObtenerResumenAsync(
            int? idInstitucion = null,
            string? modalidad = null,
            string? estado = null,
            int? mes = null,
            int? ano = null,
            DateTime? fechaInicio = null, 
            DateTime? fechaFin = null)
        {
            try
            {
                var queryPrestamos = _context.Prestamos
                    .Include(p => p.Cliente)
                    .AsNoTracking()
                    .AsQueryable();

                var queryPagos = _context.Pagos
                    .Include(p => p.Prestamo)
                        .ThenInclude(pr => pr.Cliente)
                    .AsNoTracking()
                    .AsQueryable();

                // 1. Filtro por Institución
                if (idInstitucion.HasValue)
                {
                    if (idInstitucion.Value == -1)
                    {
                        queryPrestamos = queryPrestamos.Where(p => p.Cliente != null && p.Cliente.IdInstitucion == null);
                        queryPagos = queryPagos.Where(p => p.Prestamo != null && p.Prestamo.Cliente != null && p.Prestamo.Cliente.IdInstitucion == null);
                    }
                    else if (idInstitucion.Value > 0)
                    {
                        queryPrestamos = queryPrestamos.Where(p => p.Cliente != null && p.Cliente.IdInstitucion == idInstitucion.Value);
                        queryPagos = queryPagos.Where(p => p.Prestamo != null && p.Prestamo.Cliente != null && p.Prestamo.Cliente.IdInstitucion == idInstitucion.Value);
                    }
                }

                // 2. Filtro por Modalidad
                if (!string.IsNullOrWhiteSpace(modalidad) && modalidad != "Todos")
                {
                    queryPrestamos = queryPrestamos.Where(p => p.ModalidadCobro == modalidad);
                    queryPagos = queryPagos.Where(p => p.Prestamo != null && p.Prestamo.ModalidadCobro == modalidad);
                }

                // 3. Filtro por Estado (Pagados vs Por Cobrar / Activos)
                if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos")
                {
                    if (estado == "Activo" || estado == "PorCobrar")
                    {
                        queryPrestamos = queryPrestamos.Where(p => p.Estado == "Activo" && p.SaldoPendiente > 0);
                        queryPagos = queryPagos.Where(p => p.Prestamo != null && p.Prestamo.Estado == "Activo");
                    }
                    else if (estado == "Pagado")
                    {
                        queryPrestamos = queryPrestamos.Where(p => p.Estado == "Pagado" || p.SaldoPendiente <= 0);
                        queryPagos = queryPagos.Where(p => p.Prestamo != null && (p.Prestamo.Estado == "Pagado" || p.Prestamo.SaldoPendiente <= 0));
                    }
                }

                // 4. Filtro por Mes y Año combinados si se especifican
                if (mes.HasValue && mes.Value > 0 && ano.HasValue && ano.Value > 0)
                {
                    var inicioMes = new DateTime(ano.Value, mes.Value, 1);
                    var finMes = new DateTime(ano.Value, mes.Value, DateTime.DaysInMonth(ano.Value, mes.Value)).AddDays(1).AddTicks(-1);
                    queryPrestamos = queryPrestamos.Where(p => p.FechaRegistro >= inicioMes && p.FechaRegistro <= finMes);
                    queryPagos = queryPagos.Where(p => p.FechaPago >= inicioMes && p.FechaPago <= finMes);
                }
                else
                {
                    if (mes.HasValue && mes.Value > 0)
                    {
                        queryPrestamos = queryPrestamos.Where(p => p.FechaRegistro.Month == mes.Value);
                        queryPagos = queryPagos.Where(p => p.FechaPago.Month == mes.Value);
                    }
                    if (ano.HasValue && ano.Value > 0)
                    {
                        queryPrestamos = queryPrestamos.Where(p => p.FechaRegistro.Year == ano.Value);
                        queryPagos = queryPagos.Where(p => p.FechaPago.Year == ano.Value);
                    }
                }

                // 5. Filtro por Rango de Fechas específico si se provee
                if (fechaInicio.HasValue)
                {
                    var inicio = fechaInicio.Value.Date;
                    queryPrestamos = queryPrestamos.Where(p => p.FechaRegistro >= inicio);
                    queryPagos = queryPagos.Where(p => p.FechaPago >= inicio);
                }

                if (fechaFin.HasValue)
                {
                    var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                    queryPrestamos = queryPrestamos.Where(p => p.FechaRegistro <= fin);
                    queryPagos = queryPagos.Where(p => p.FechaPago <= fin);
                }

                var totalClientes = await _context.Clientes.CountAsync();
                var clientesActivos = await _context.Clientes.CountAsync(c => c.EstaHabilitado);
                var totalPrestamos = await queryPrestamos.CountAsync();
                var prestamosActivos = await queryPrestamos.CountAsync(p => p.Estado == "Activo" && p.SaldoPendiente > 0);
                var prestamosPagados = await queryPrestamos.CountAsync(p => p.Estado == "Pagado" || p.SaldoPendiente <= 0);

                var totalCapital = await queryPrestamos.SumAsync(p => (decimal?)p.Monto) ?? 0m;
                var totalCartera = await queryPrestamos.SumAsync(p => (decimal?)p.MontoTotal) ?? 0m;
                var saldoPendiente = await queryPrestamos.SumAsync(p => (decimal?)p.SaldoPendiente) ?? 0m;
                var totalRecaudado = await queryPagos.SumAsync(p => (decimal?)p.MontoAbonado) ?? 0m;

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

                resumen.DistribucionModalidades = await queryPrestamos
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

        public async Task<Result<byte[]>> GenerarReporteExcelAsync(
            int? idInstitucion = null,
            string? modalidad = null,
            string? estado = null,
            int? mes = null,
            int? ano = null,
            DateTime? fechaInicio = null, 
            DateTime? fechaFin = null, 
            string titulo = "Reporte Ejecutivo de Cartera de Préstamos")
        {
            try
            {
                var query = _context.Prestamos
                    .Include(p => p.Cliente)
                        .ThenInclude(c => c.Institucion)
                    .Include(p => p.Pagos)
                    .AsNoTracking()
                    .AsQueryable();

                if (idInstitucion.HasValue)
                {
                    if (idInstitucion.Value == -1)
                        query = query.Where(p => p.Cliente != null && p.Cliente.IdInstitucion == null);
                    else if (idInstitucion.Value > 0)
                        query = query.Where(p => p.Cliente != null && p.Cliente.IdInstitucion == idInstitucion.Value);
                }

                if (!string.IsNullOrWhiteSpace(modalidad) && modalidad != "Todos")
                {
                    query = query.Where(p => p.ModalidadCobro == modalidad);
                }

                if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos")
                {
                    if (estado == "Activo" || estado == "PorCobrar")
                        query = query.Where(p => p.Estado == "Activo" && p.SaldoPendiente > 0);
                    else if (estado == "Pagado")
                        query = query.Where(p => p.Estado == "Pagado" || p.SaldoPendiente <= 0);
                }

                if (mes.HasValue && mes.Value > 0 && ano.HasValue && ano.Value > 0)
                {
                    var inicioMes = new DateTime(ano.Value, mes.Value, 1);
                    var finMes = new DateTime(ano.Value, mes.Value, DateTime.DaysInMonth(ano.Value, mes.Value)).AddDays(1).AddTicks(-1);
                    query = query.Where(p => p.FechaRegistro >= inicioMes && p.FechaRegistro <= finMes);
                }
                else
                {
                    if (mes.HasValue && mes.Value > 0)
                        query = query.Where(p => p.FechaRegistro.Month == mes.Value);
                    if (ano.HasValue && ano.Value > 0)
                        query = query.Where(p => p.FechaRegistro.Year == ano.Value);
                }

                if (fechaInicio.HasValue)
                {
                    var inicio = fechaInicio.Value.Date;
                    query = query.Where(p => p.FechaRegistro >= inicio);
                }

                if (fechaFin.HasValue)
                {
                    var fin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(p => p.FechaRegistro <= fin);
                }

                var prestamos = await query
                    .OrderByDescending(p => p.FechaRegistro)
                    .ToListAsync();

                using (var libro = new XLWorkbook())
                {
                    var hoja = libro.Worksheets.Add("Cartera de Préstamos");

                    // Título Superior
                    var rangoTitulo = hoja.Range("A1:I1");
                    rangoTitulo.Merge().Value = titulo.ToUpper();
                    rangoTitulo.Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    rangoTitulo.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                    // Subtítulo con Filtros
                    var listaFiltrosTexto = new List<string>();
                    if (idInstitucion.HasValue)
                        listaFiltrosTexto.Add(idInstitucion.Value == -1 ? "INSTITUCIÓN: Sin Institución" : "INSTITUCIÓN: Filtrada");
                    if (!string.IsNullOrWhiteSpace(modalidad) && modalidad != "Todos")
                        listaFiltrosTexto.Add($"MODALIDAD: {modalidad}");
                    if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos")
                        listaFiltrosTexto.Add($"ESTADO: {estado}");
                    if (fechaInicio.HasValue && fechaFin.HasValue)
                        listaFiltrosTexto.Add($"PERÍODO: {fechaInicio.Value:dd/MM/yyyy} AL {fechaFin.Value:dd/MM/yyyy}");
                    else if (mes.HasValue && ano.HasValue)
                        listaFiltrosTexto.Add($"MES: {mes.Value}/{ano.Value}");
                    else
                        listaFiltrosTexto.Add("PERÍODO: Histórico General");

                    var rangoPeriodo = hoja.Range("A2:I2");
                    rangoPeriodo.Merge().Value = string.Join(" | ", listaFiltrosTexto);
                    rangoPeriodo.Style.Font.SetItalic().Font.SetFontSize(9).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    rangoPeriodo.Style.Fill.BackgroundColor = XLColor.FromHtml("#FAFAFA");

                    // Cabeceras de Columnas estilo SIS.INV
                    string[] columnas = { "ID", "CLIENTE", "INSTITUCIÓN", "MODALIDAD", "CAPITAL (BS)", "% INT.", "TOTAL DEUDA (BS)", "TOTAL ABONADO (BS)", "SALDO PENDIENTE (BS)" };
                    for (int i = 0; i < columnas.Length; i++)
                    {
                        var celda = hoja.Cell(4, i + 1);
                        celda.Value = columnas[i];
                        celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
                        celda.Style.Font.FontColor = XLColor.White;
                        celda.Style.Font.Bold = true;
                        celda.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    }

                    int fila = 5;
                    decimal sumCapital = 0m;
                    decimal sumTotal = 0m;
                    decimal sumAbonado = 0m;
                    decimal sumSaldo = 0m;

                    foreach (var p in prestamos)
                    {
                        decimal abonado = p.Pagos.Sum(x => x.MontoAbonado);
                        sumCapital += p.Monto;
                        sumTotal += p.MontoTotal;
                        sumAbonado += abonado;
                        sumSaldo += p.SaldoPendiente;

                        hoja.Cell(fila, 1).Value = p.IdPrestamo;
                        hoja.Cell(fila, 2).Value = p.Cliente?.Nombre ?? "N/A";
                        hoja.Cell(fila, 3).Value = p.Cliente?.Institucion?.Nombre ?? "Sin Institución";
                        hoja.Cell(fila, 4).Value = p.ModalidadCobro;
                        hoja.Cell(fila, 5).Value = p.Monto;
                        hoja.Cell(fila, 6).Value = $"{p.PorcentajeInteres}%";
                        hoja.Cell(fila, 7).Value = p.MontoTotal;
                        hoja.Cell(fila, 8).Value = abonado;
                        hoja.Cell(fila, 9).Value = p.SaldoPendiente;

                        fila++;
                    }

                    // Fila de Totales
                    if (prestamos.Count > 0)
                    {
                        var celdaTotalLabel = hoja.Cell(fila, 2);
                        celdaTotalLabel.Value = "TOTALES CONSOLIDADOS";
                        celdaTotalLabel.Style.Font.Bold = true;

                        hoja.Cell(fila, 5).Value = sumCapital;
                        hoja.Cell(fila, 5).Style.Font.Bold = true;

                        hoja.Cell(fila, 7).Value = sumTotal;
                        hoja.Cell(fila, 7).Style.Font.Bold = true;

                        hoja.Cell(fila, 8).Value = sumAbonado;
                        hoja.Cell(fila, 8).Style.Font.Bold = true;

                        hoja.Cell(fila, 9).Value = sumSaldo;
                        hoja.Cell(fila, 9).Style.Font.Bold = true;

                        var filaTotalesRango = hoja.Range(fila, 1, fila, 9);
                        filaTotalesRango.Style.Fill.BackgroundColor = XLColor.FromHtml("#ECEFF1");
                        filaTotalesRango.Style.Font.Bold = true;
                    }

                    hoja.Columns().AdjustToContents();
                    var tabla = hoja.Range(4, 1, Math.Max(4, fila), 9);
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
