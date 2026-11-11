using API1.Common;
using API1.Context;
using API1.DTOs;
using API1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API1.RN
{
    public class AuthRN
    {
        private readonly IConfiguration _configuration;
        private readonly YacPrestamosDbContext _context;

        public AuthRN(IConfiguration configuration, YacPrestamosDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<Result<AuthResponseDTO>> LoginAsync(LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Result.Fail400<AuthResponseDTO>("Credenciales incompletas.");
            }

            var loginInput = request.UserName.Trim();
            var user = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.CorreoElectronico == loginInput || u.NombreUsuario == loginInput);

            if (user == null)
            {
                return Result.Fail401<AuthResponseDTO>("Usuario o contraseña incorrectos.");
            }

            if (!user.EstaHabilitado)
            {
                return Result.Fail401<AuthResponseDTO>("El usuario se encuentra deshabilitado por un administrador.");
            }

            if (user.ContrasenaHash != request.Password)
            {
                return Result.Fail401<AuthResponseDTO>("Usuario o contraseña incorrectos.");
            }

            var tokenResult = GenerateJwtToken(
                user.IdUsuario.ToString(),
                user.NombreUsuario,
                user.CorreoElectronico,
                user.Rol
            );

            return Result.Ok(tokenResult, "Inicio de sesión exitoso.");
        }

        public async Task<Result<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Result.Fail400<AuthResponseDTO>("Todos los campos son obligatorios.");
            }

            var email = request.Email.Trim();
            var username = request.UserName.Trim();

            var exists = await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == email || u.NombreUsuario == username);
            if (exists)
            {
                return Result.Fail400<AuthResponseDTO>("El nombre de usuario o correo electrónico ya está registrado.");
            }

            var newUser = new Usuario
            {
                NombreUsuario = username,
                CorreoElectronico = email,
                ContrasenaHash = request.Password,
                Rol = string.IsNullOrWhiteSpace(request.Role) ? "Operador" : request.Role,
                EstaHabilitado = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(newUser);
            await _context.SaveChangesAsync();

            var tokenResult = GenerateJwtToken(newUser.IdUsuario.ToString(), newUser.NombreUsuario, newUser.CorreoElectronico, newUser.Rol);
            return Result.Ok(tokenResult, "Usuario registrado exitosamente.");
        }

        public async Task<Result<UserProfileDTO>> GetProfileAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(userName))
            {
                return await Task.FromResult(Result.Fail401<UserProfileDTO>("Usuario no autenticado o token inválido."));
            }

            var profile = new UserProfileDTO
            {
                Id = userId,
                UserName = userName,
                Email = email,
                Role = role
            };

            return await Task.FromResult(Result.Ok(profile));
        }

        private AuthResponseDTO GenerateJwtToken(string userId, string userName, string email, string role)
        {
            var keyString = _configuration["Jwt:Key"] ?? "TuSuperClaveSecretaDeDesarrolloParaProyectosPersonales123456!";
            var issuer = _configuration["Jwt:Issuer"] ?? "WEB_API_N10";
            var audience = _configuration["Jwt:Audience"] ?? "WEB_API_N10";
            var expiresMinutes = int.TryParse(_configuration["Jwt:Expires"] ?? _configuration["Jwt:ExpiresInMinutes"], out var mins) ? mins : 1440;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserName = userName,
                Email = email,
                Role = role,
                ExpiresAt = expires
            };
        }
    }
}
