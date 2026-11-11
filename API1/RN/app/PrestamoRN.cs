using API1.Common;
using API1.Context;
using API1.DTOs.app;
using API1.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API1.RN.app
{
    public class PrestamoRN
    {
        private readonly YacPrestamosDbContext _context;
        private readonly IMapper _mapper;

        public PrestamoRN(YacPrestamosDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<PrestamoDTO>>> GetPrestamos(bool soloActivos = false)
        {
            var query = _context.Prestamos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .AsQueryable();

            if (soloActivos)
            {
                query = query.Where(p => p.Estado == "Activo" && p.SaldoPendiente > 0);
            }

            var entidades = await query.OrderByDescending(p => p.IdPrestamo).ToListAsync();
            var dtos = _mapper.Map<List<PrestamoDTO>>(entidades);

            return Result.Ok(dtos, "Préstamos cargados exitosamente.");
        }

        public async Task<Result<List<PrestamoDTO>>> GetPrestamosPorCliente(int idCliente)
        {
            var entidades = await _context.Prestamos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .Where(p => p.IdCliente == idCliente)
                .OrderByDescending(p => p.IdPrestamo)
                .ToListAsync();

            var dtos = _mapper.Map<List<PrestamoDTO>>(entidades);
            return Result.Ok(dtos, "Préstamos del cliente obtenidos exitosamente.");
        }

        public async Task<Result<PrestamoDTO>> GetPrestamoById(int id)
        {
            var prestamo = await _context.Prestamos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.IdPrestamo == id);

            if (prestamo == null)
            {
                return Result.Fail404<PrestamoDTO>("Préstamo no encontrado.");
            }

            return Result.Ok(_mapper.Map<PrestamoDTO>(prestamo));
        }

        public async Task<Result<PrestamoDTO>> PostPrestamo(PrestamoCreacionDTO dto)
        {
            if (dto.IdCliente <= 0)
            {
                return Result.Fail400<PrestamoDTO>("Debe seleccionar un cliente válido.");
            }

            if (dto.Monto <= 0)
            {
                return Result.Fail400<PrestamoDTO>("El monto del préstamo debe ser mayor a 0.");
            }

            if (dto.PorcentajeInteres < 0)
            {
                return Result.Fail400<PrestamoDTO>("El porcentaje de interés no puede ser negativo.");
            }

            var cliente = await _context.Clientes.FindAsync(dto.IdCliente);
            if (cliente == null)
            {
                return Result.Fail404<PrestamoDTO>("El cliente seleccionado no existe.");
            }

            if (!cliente.EstaHabilitado)
            {
                return Result.Fail400<PrestamoDTO>("No se puede otorgar un préstamo a un cliente deshabilitado.");
            }

            // Cálculo financiero de la versión original
            decimal montoTotal = dto.Monto + (dto.Monto * (dto.PorcentajeInteres / 100m));
            decimal saldoPendiente = montoTotal;

            var entidad = _mapper.Map<Prestamo>(dto);
            entidad.MontoTotal = Math.Round(montoTotal, 2);
            entidad.SaldoPendiente = Math.Round(saldoPendiente, 2);
            entidad.Estado = "Activo";
            entidad.FechaRegistro = DateTime.UtcNow;

            _context.Prestamos.Add(entidad);
            await _context.SaveChangesAsync();

            // Cargar datos de navegación para responder DTO completo
            await _context.Entry(entidad).Reference(p => p.Cliente).LoadAsync();

            return Result.Ok(_mapper.Map<PrestamoDTO>(entidad), "Préstamo registrado exitosamente.");
        }

        public async Task<Result<PrestamoDTO>> PutPrestamo(PrestamoDTO dto)
        {
            if (dto.IdPrestamo <= 0)
            {
                return Result.Fail400<PrestamoDTO>("Identificador de préstamo inválido.");
            }

            var prestamo = await _context.Prestamos.FindAsync(dto.IdPrestamo);
            if (prestamo == null)
            {
                return Result.Fail404<PrestamoDTO>("Préstamo no encontrado.");
            }

            // Regla de negocio de VersionAntigua/ModalPrestamo.js:
            // No permitir editar préstamos ya pagados/completados
            if (prestamo.Estado == "Pagado" || prestamo.Estado == "Completado" || prestamo.SaldoPendiente <= 0)
            {
                return Result.Fail400<PrestamoDTO>("Este préstamo ya está liquidado; no se permite su modificación.");
            }

            prestamo.ModalidadCobro = dto.ModalidadCobro;
            prestamo.Estado = dto.Estado;

            // Si se recalcula monto o interés
            if (dto.Monto > 0 && dto.Monto != prestamo.Monto || dto.PorcentajeInteres != prestamo.PorcentajeInteres)
            {
                prestamo.Monto = dto.Monto;
                prestamo.PorcentajeInteres = dto.PorcentajeInteres;
                prestamo.MontoTotal = Math.Round(dto.Monto + (dto.Monto * (dto.PorcentajeInteres / 100m)), 2);
                prestamo.SaldoPendiente = prestamo.MontoTotal;
            }

            await _context.SaveChangesAsync();
            await _context.Entry(prestamo).Reference(p => p.Cliente).LoadAsync();

            return Result.Ok(_mapper.Map<PrestamoDTO>(prestamo), "Préstamo actualizado exitosamente.");
        }
    }
}

