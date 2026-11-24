using API1.Common;
using API1.Context;
using API1.DTOs.app;
using API1.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API1.RN.app
{
    public class InstitucionRN
    {
        private readonly YacPrestamosDbContext _context;
        private readonly IMapper _mapper;

        public InstitucionRN(YacPrestamosDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<InstitucionDTO>>> GetInstituciones(bool soloActivos = false)
        {
            var query = _context.Instituciones.AsNoTracking();

            if (soloActivos)
            {
                query = query.Where(i => i.EstaHabilitado);
            }

            var entidades = await query.OrderBy(i => i.Nombre).ToListAsync();
            var dtos = _mapper.Map<List<InstitucionDTO>>(entidades);

            return Result.Ok(dtos, "Instituciones cargadas exitosamente.");
        }

        public async Task<Result<ResponseListDTO<InstitucionDTO>>> GetInstitucionesPaginadoAndBuscador(
            int pageNumber = 1,
            int pageSize = 10,
            string? q = null,
            bool soloActivos = false,
            CancellationToken c = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Instituciones.AsNoTracking();

            if (soloActivos)
            {
                query = query.Where(i => i.EstaHabilitado);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                int.TryParse(q, out int idBuscado);

                query = query.Where(i =>
                    i.Nombre.ToLower().Contains(q) ||
                    (i.Direccion != null && i.Direccion.ToLower().Contains(q)) ||
                    i.IdInstitucion == idBuscado
                );
            }

            var totalItems = await query.CountAsync(c);
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var entidades = await query
                .OrderBy(i => i.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(c);

            var items = _mapper.Map<List<InstitucionDTO>>(entidades);

            return Result.Ok(new ResponseListDTO<InstitucionDTO>
            {
                page = pageNumber,
                quanty = pageSize,
                total = totalPages,
                value = items
            }, "Instituciones paginadas cargadas exitosamente.");
        }

        public async Task<Result<InstitucionDTO>> GetInstitucionById(int id)
        {
            var entidad = await _context.Instituciones.AsNoTracking().FirstOrDefaultAsync(i => i.IdInstitucion == id);
            if (entidad == null)
            {
                return Result.Fail404<InstitucionDTO>("Institución no encontrada.");
            }

            return Result.Ok(_mapper.Map<InstitucionDTO>(entidad));
        }

        public async Task<Result<InstitucionDTO>> PostInstitucion(InstitucionDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return Result.Fail400<InstitucionDTO>("El nombre de la institución es obligatorio.");
            }

            var entidad = _mapper.Map<Institucion>(dto);
            entidad.Nombre = entidad.Nombre.Trim();
            entidad.Direccion = string.IsNullOrWhiteSpace(entidad.Direccion) ? null : entidad.Direccion.Trim();
            entidad.FechaRegistro = DateTime.UtcNow;
            entidad.EstaHabilitado = true;

            _context.Instituciones.Add(entidad);
            await _context.SaveChangesAsync();

            return Result.Ok(_mapper.Map<InstitucionDTO>(entidad), "Institución registrada exitosamente.");
        }

        public async Task<Result<InstitucionDTO>> PutInstitucion(InstitucionDTO dto)
        {
            if (dto.IdInstitucion <= 0)
            {
                return Result.Fail400<InstitucionDTO>("Identificador de institución inválido.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return Result.Fail400<InstitucionDTO>("El nombre de la institución es obligatorio.");
            }

            var entidad = await _context.Instituciones.FindAsync(dto.IdInstitucion);
            if (entidad == null)
            {
                return Result.Fail404<InstitucionDTO>("Institución no encontrada.");
            }

            entidad.Nombre = dto.Nombre.Trim();
            entidad.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
            entidad.EstaHabilitado = dto.EstaHabilitado;

            await _context.SaveChangesAsync();

            return Result.Ok(_mapper.Map<InstitucionDTO>(entidad), "Institución actualizada exitosamente.");
        }

        public async Task<Result<InstitucionDTO>> CambiarEstado(int id)
        {
            var entidad = await _context.Instituciones.FindAsync(id);
            if (entidad == null)
            {
                return Result.Fail404<InstitucionDTO>("Institución no encontrada.");
            }

            entidad.EstaHabilitado = !entidad.EstaHabilitado;
            await _context.SaveChangesAsync();

            var mensaje = entidad.EstaHabilitado ? "Institución habilitada correctamente." : "Institución deshabilitada correctamente.";
            return Result.Ok(_mapper.Map<InstitucionDTO>(entidad), mensaje);
        }
    }
}
