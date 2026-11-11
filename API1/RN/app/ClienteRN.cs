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
            var query = _context.Clientes.AsNoTracking();

            if (soloActivos)
            {
                query = query.Where(c => c.EstaHabilitado);
            }

            var entidades = await query.OrderBy(c => c.Nombre).ToListAsync();
            var dtos = _mapper.Map<List<ClienteDTO>>(entidades);

            return Result.Ok(dtos, "Clientes cargados exitosamente.");
        }

        public async Task<Result<ClienteDTO>> GetClienteById(int id)
        {
            var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
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

            var entidad = _mapper.Map<Cliente>(dto);
            entidad.Nombre = entidad.Nombre.Trim();
            entidad.Celular = entidad.Celular.Trim();
            entidad.Correo = string.IsNullOrWhiteSpace(entidad.Correo) ? null : entidad.Correo.Trim();
            entidad.Direccion = string.IsNullOrWhiteSpace(entidad.Direccion) ? null : entidad.Direccion.Trim();
            entidad.FechaRegistro = DateTime.UtcNow;
            entidad.EstaHabilitado = true;

            _context.Clientes.Add(entidad);
            await _context.SaveChangesAsync();

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
            cliente.EstaHabilitado = dto.EstaHabilitado;

            await _context.SaveChangesAsync();

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

