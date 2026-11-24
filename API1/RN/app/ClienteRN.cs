using API1.Common;
using API1.Context;
using API1.DTOs.app;
using API1.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API1.RN.app
{
    public class ClienteRN
    {
        private readonly YacPrestamosDbContext _context;
        private readonly IMapper _mapper;

        public ClienteRN(YacPrestamosDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ClienteDTO>>> GetClientes(bool soloActivos = false)
        {
            var query = _context.Clientes
                .Include(c => c.Institucion)
                .AsNoTracking();

            if (soloActivos)
            {
                query = query.Where(c => c.EstaHabilitado);
            }

            var entidades = await query.OrderBy(c => c.Nombre).ToListAsync();
            var dtos = _mapper.Map<List<ClienteDTO>>(entidades);

            return Result.Ok(dtos, "Clientes cargados exitosamente.");
        }

        public async Task<Result<ResponseListDTO<ClienteDTO>>> GetClientesPaginadoAndBuscador(
            int pageNumber = 1,
            int pageSize = 10,
            string? q = null,
            int? idInstitucion = null,
            bool soloActivos = false,
            CancellationToken c = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Clientes
                .Include(c => c.Institucion)
                .AsNoTracking();

            if (soloActivos)
            {
                query = query.Where(c => c.EstaHabilitado);
            }

            if (idInstitucion.HasValue)
            {
                if (idInstitucion.Value == -1)
                {
                    query = query.Where(c => c.IdInstitucion == null);
                }
                else if (idInstitucion.Value > 0)
                {
                    query = query.Where(c => c.IdInstitucion == idInstitucion.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                int.TryParse(q, out int idBuscado);

                query = query.Where(c =>
                    c.Nombre.ToLower().Contains(q) ||
                    c.Celular.ToLower().Contains(q) ||
                    (c.Correo != null && c.Correo.ToLower().Contains(q)) ||
                    (c.NumeroTarjetaCredito != null && c.NumeroTarjetaCredito.ToLower().Contains(q)) ||
                    (c.Institucion != null && c.Institucion.Nombre.ToLower().Contains(q)) ||
                    c.IdCliente == idBuscado
                );
            }

            var totalItems = await query.CountAsync(c);
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var entidades = await query
                .OrderBy(c => c.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(c);

            var items = _mapper.Map<List<ClienteDTO>>(entidades);

            return Result.Ok(new ResponseListDTO<ClienteDTO>
            {
                page = pageNumber,
                quanty = pageSize,
                total = totalPages,
                value = items
            }, "Clientes paginados cargados exitosamente.");
        }

        public async Task<Result<ClienteDTO>> GetClienteById(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Institucion)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
            {
                return Result.Fail404<ClienteDTO>("Cliente no encontrado.");
            }

            return Result.Ok(_mapper.Map<ClienteDTO>(cliente));
        }

        public async Task<Result<ClienteDTO>> PostCliente(ClienteDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Celular))
            {
                return Result.Fail400<ClienteDTO>("El nombre y el celular son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(dto.NumeroTarjetaCredito))
            {
                return Result.Fail400<ClienteDTO>("El número de tarjeta de crédito es obligatorio.");
            }

            var entidad = _mapper.Map<Cliente>(dto);
            entidad.Nombre = entidad.Nombre.Trim();
            entidad.Celular = entidad.Celular.Trim();
            entidad.Correo = string.IsNullOrWhiteSpace(entidad.Correo) ? null : entidad.Correo.Trim();
            entidad.Direccion = string.IsNullOrWhiteSpace(entidad.Direccion) ? null : entidad.Direccion.Trim();
            entidad.NumeroTarjetaCredito = dto.NumeroTarjetaCredito.Trim();
            entidad.FechaInicioContrato = dto.FechaInicioContrato;
            entidad.FechaFinContrato = dto.FechaFinContrato;
            entidad.IdInstitucion = (dto.IdInstitucion.HasValue && dto.IdInstitucion.Value > 0) ? dto.IdInstitucion : null;
            entidad.FechaRegistro = DateTime.UtcNow;
            entidad.EstaHabilitado = true;

            _context.Clientes.Add(entidad);
            await _context.SaveChangesAsync();

            // Cargar relación de institución para devolver el DTO completo
            if (entidad.IdInstitucion.HasValue)
            {
                await _context.Entry(entidad).Reference(c => c.Institucion).LoadAsync();
            }

            return Result.Ok(_mapper.Map<ClienteDTO>(entidad), "Cliente registrado exitosamente.");
        }

        public async Task<Result<ClienteDTO>> PutCliente(ClienteDTO dto)
        {
            if (dto.IdCliente <= 0)
            {
                return Result.Fail400<ClienteDTO>("Identificador de cliente inválido.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Celular))
            {
                return Result.Fail400<ClienteDTO>("El nombre y el celular son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(dto.NumeroTarjetaCredito))
            {
                return Result.Fail400<ClienteDTO>("El número de tarjeta de crédito es obligatorio.");
            }

            var cliente = await _context.Clientes.FindAsync(dto.IdCliente);
            if (cliente == null)
            {
                return Result.Fail404<ClienteDTO>("Cliente no encontrado.");
            }

            cliente.Nombre = dto.Nombre.Trim();
            cliente.Celular = dto.Celular.Trim();
            cliente.Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim();
            cliente.EstadoCivil = dto.EstadoCivil;
            cliente.FechaNacimiento = dto.FechaNacimiento;
            cliente.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
            cliente.NumeroTarjetaCredito = dto.NumeroTarjetaCredito.Trim();
            cliente.FechaInicioContrato = dto.FechaInicioContrato;
            cliente.FechaFinContrato = dto.FechaFinContrato;
            cliente.IdInstitucion = (dto.IdInstitucion.HasValue && dto.IdInstitucion.Value > 0) ? dto.IdInstitucion : null;
            cliente.EstaHabilitado = dto.EstaHabilitado;

            await _context.SaveChangesAsync();

            if (cliente.IdInstitucion.HasValue)
            {
                await _context.Entry(cliente).Reference(c => c.Institucion).LoadAsync();
            }

            return Result.Ok(_mapper.Map<ClienteDTO>(cliente), "Cliente actualizado exitosamente.");
        }

        public async Task<Result<ClienteDTO>> CambiarEstado(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return Result.Fail404<ClienteDTO>("Cliente no encontrado.");
            }

            cliente.EstaHabilitado = !cliente.EstaHabilitado;
            await _context.SaveChangesAsync();

            var mensaje = cliente.EstaHabilitado ? "Cliente habilitado correctamente." : "Cliente deshabilitado correctamente.";
            return Result.Ok(_mapper.Map<ClienteDTO>(cliente), mensaje);
        }
    }
}
