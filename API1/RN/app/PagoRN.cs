using API1.Common;
using API1.Context;
using API1.DTOs.app;
using API1.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API1.RN.app
{
    public class PagoRN
    {
        private readonly YacPrestamosDbContext _context;
        private readonly IMapper _mapper;

        public PagoRN(YacPrestamosDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagoDTO>> RegistrarAbono(RegistrarAbonoDTO dto)
        {
            if (dto.IdPrestamo <= 0)
            {
                return Result.Fail400<PagoDTO>("Debe indicar un préstamo válido.");
            }

            if (dto.MontoAbonado <= 0)
            {
                return Result.Fail400<PagoDTO>("El monto a abonar debe ser mayor a 0.");
            }

            var prestamo = await _context.Prestamos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.IdPrestamo == dto.IdPrestamo);

            if (prestamo == null)
            {
                return Result.Fail404<PagoDTO>("El préstamo no existe.");
            }

            if (prestamo.SaldoPendiente <= 0 || prestamo.Estado == "Pagado" || prestamo.Estado == "Completado")
            {
                return Result.Fail400<PagoDTO>("El préstamo ya se encuentra liquidado en su totalidad.");
            }

            if (dto.MontoAbonado > prestamo.SaldoPendiente)
            {
                return Result.Fail400<PagoDTO>($"El monto a abonar (Bs. {dto.MontoAbonado:F2}) no puede superar el saldo pendiente (Bs. {prestamo.SaldoPendiente:F2}).");
            }

            // 1. Crear el registro del Pago
            var pago = new Pago
            {
                IdPrestamo = dto.IdPrestamo,
                MontoAbonado = Math.Round(dto.MontoAbonado, 2),
                FechaPago = DateTime.UtcNow
            };
            _context.Pagos.Add(pago);

            // 2. Descontar el saldo del préstamo
            prestamo.SaldoPendiente = Math.Round(prestamo.SaldoPendiente - dto.MontoAbonado, 2);
            if (prestamo.SaldoPendiente <= 0)
            {
                prestamo.SaldoPendiente = 0;
                prestamo.Estado = "Pagado";
            }

            await _context.SaveChangesAsync();

            return Result.Ok(_mapper.Map<PagoDTO>(pago), "Pago de cuota registrado exitosamente.");
        }

        public async Task<Result<ResumenHistorialPagosDTO>> GetPagosPorPrestamo(int idPrestamo)
        {
            var prestamo = await _context.Prestamos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .Include(p => p.Pagos)
                .FirstOrDefaultAsync(p => p.IdPrestamo == idPrestamo);

            if (prestamo == null)
            {
                return Result.Fail404<ResumenHistorialPagosDTO>("Préstamo no encontrado.");
            }

            var listaPagos = prestamo.Pagos.OrderByDescending(p => p.FechaPago).ToList();
            var dtosPagos = _mapper.Map<List<PagoDTO>>(listaPagos);
            decimal totalAbonado = listaPagos.Sum(p => p.MontoAbonado);

            var resumen = new ResumenHistorialPagosDTO
            {
                IdPrestamo = prestamo.IdPrestamo,
                NombreCliente = prestamo.Cliente != null ? prestamo.Cliente.Nombre : "Sin titular",
                MontoTotalPrestamo = prestamo.MontoTotal,
                SaldoPendiente = prestamo.SaldoPendiente,
                TotalAbonado = totalAbonado,
                Pagos = dtosPagos
            };

            return Result.Ok(resumen, "Historial de pagos cargado exitosamente.");
        }
    }
}

