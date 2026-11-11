using API1.Common;
using API1.Context;
using API1.DTOs.app;
using API1.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API1.RN.app
{
    public class UsuarioRN
    {
        private readonly YacPrestamosDbContext _context;
        private readonly IMapper _mapper;

        public UsuarioRN(YacPrestamosDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<UsuarioDTO>>> GetUsuarios()
        {
            var entidades = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.IdUsuario)
                .ToListAsync();

            var dtos = _mapper.Map<List<UsuarioDTO>>(entidades);
            return Result.Ok(dtos, "Usuarios cargados exitosamente.");
        }

        public async Task<Result<UsuarioDTO>> GetUsuarioById(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return Result.Fail404<UsuarioDTO>("Usuario no encontrado.");
            }

            return Result.Ok(_mapper.Map<UsuarioDTO>(usuario));
        }

        public async Task<Result<UsuarioDTO>> PostUsuario(UsuarioCreacionDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreUsuario) || string.IsNullOrWhiteSpace(dto.CorreoElectronico) || string.IsNullOrWhiteSpace(dto.Contrasena))
            {
                return Result.Fail400<UsuarioDTO>("Nombre, correo y contraseña son obligatorios.");
            }

            var correo = dto.CorreoElectronico.Trim();
            var username = dto.NombreUsuario.Trim();

            var existe = await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == correo || u.NombreUsuario == username);
            if (existe)
            {
                return Result.Fail400<UsuarioDTO>("El nombre de usuario o correo electrónico ya está registrado.");
            }

            var entidad = _mapper.Map<Usuario>(dto);
            entidad.NombreUsuario = username;
            entidad.CorreoElectronico = correo;
            entidad.ContrasenaHash = dto.Contrasena;
            entidad.Rol = string.IsNullOrWhiteSpace(dto.Rol) ? "Operador" : dto.Rol;
            entidad.EstaHabilitado = true;
            entidad.FechaCreacion = DateTime.UtcNow;

            _context.Usuarios.Add(entidad);
            await _context.SaveChangesAsync();

            return Result.Ok(_mapper.Map<UsuarioDTO>(entidad), "Usuario creado exitosamente.");
        }

        public async Task<Result<UsuarioDTO>> PutUsuario(UsuarioEdicionDTO dto)
        {
            if (dto.IdUsuario <= 0)
            {
                return Result.Fail400<UsuarioDTO>("Identificador de usuario inválido.");
            }

            var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            if (usuario == null)
            {
                return Result.Fail404<UsuarioDTO>("Usuario no encontrado.");
            }

            var correo = dto.CorreoElectronico.Trim();
            var username = dto.NombreUsuario.Trim();

            var duplicado = await _context.Usuarios.AnyAsync(u => u.IdUsuario != dto.IdUsuario && (u.CorreoElectronico == correo || u.NombreUsuario == username));
            if (duplicado)
            {
                return Result.Fail400<UsuarioDTO>("El correo o nombre de usuario ya pertenece a otra cuenta.");
            }

            usuario.NombreUsuario = username;
            usuario.CorreoElectronico = correo;
            usuario.Rol = dto.Rol;
            usuario.EstaHabilitado = dto.EstaHabilitado;

            if (!string.IsNullOrWhiteSpace(dto.NuevaContrasena))
            {
                usuario.ContrasenaHash = dto.NuevaContrasena;
            }

            await _context.SaveChangesAsync();

            return Result.Ok(_mapper.Map<UsuarioDTO>(usuario), "Usuario actualizado exitosamente.");
        }

        public async Task<Result<UsuarioDTO>> CambiarEstado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return Result.Fail404<UsuarioDTO>("Usuario no encontrado.");
            }

            usuario.EstaHabilitado = !usuario.EstaHabilitado;
            await _context.SaveChangesAsync();

            var mensaje = usuario.EstaHabilitado ? "Usuario habilitado correctamente." : "Usuario deshabilitado correctamente.";
            return Result.Ok(_mapper.Map<UsuarioDTO>(usuario), mensaje);
        }
    }
}
