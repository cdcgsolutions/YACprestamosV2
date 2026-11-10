using API1.Common;
using API1.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace API1.RN
{
    public class AuthRN
    {
        private readonly IConfiguration _configuration;
        // private readonly [TuPropioDbContext] _context;

        public AuthRN(IConfiguration configuration /*, [TuPropioDbContext] context */)
        {
            _configuration = configuration;
            // _context = context; 
        }

        public async Task<Result<AuthResponseDTO>> LoginAsync(LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Result.Fail400<AuthResponseDTO>("Credenciales incompletas.");
            }

            /* EJEMPLO: LÓGICA DE LOGIN CON TU DBCONTEXT 
             * 
             * // 1. Buscar el usuario en tu tabla correspondiente (ej. Users, Usuarios, Empleados)
             * var user = await _context.Users
             *     .Include(u => u.Role) // Si tienes tabla de roles relacionada
             *     .FirstOrDefaultAsync(u => u.UserName == request.UserName || u.Email == request.UserName);
             * 
             * // 2. Validar si el usuario existe
             * if (user == null)
             * {
             *     return Result.Fail401<AuthResponseDTO>("Usuario o contraseña incorrectos.");
             * }
             * 
             * // 3. Validar contraseña contra el PasswordHash almacenado en la BD
             * // (Ejemplo usando BCrypt.Net o tu propio algoritmo de hash):
             * bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
             * if (!isPasswordValid)
             * {
             *     return Result.Fail401<AuthResponseDTO>("Usuario o contraseña incorrectos.");
             * }
             * 
             * // 4. Generar el Token con los datos REALES certificados por tu base de datos
             * var tokenResult = GenerateJwtToken(
             *     user.Id.ToString(), 
             *     user.UserName, 
             *     user.Email, 
             *     user.Role?.Name ?? "User"
             * );
             * 
             * return Result.Ok(tokenResult, "Inicio de sesión exitoso.");
             */

            //  MODO ACTUAL 
            var email = !string.IsNullOrWhiteSpace(request.Email)
                ? request.Email
                : (request.UserName.Contains('@') ? request.UserName : string.Empty);

            var role = !string.IsNullOrWhiteSpace(request.Role)
                ? request.Role
                : "User";

            var tokenDemo = GenerateJwtToken(Guid.NewGuid().ToString(), request.UserName, email, role);
            return await Task.FromResult(Result.Ok(tokenDemo, "Inicio de sesión exitoso."));
        }

        public async Task<Result<AuthResponseDTO>> RegisterAsync(RegisterRequestDTO request)
        {
            /*  EJEMPLO: LÓGICA DE REGISTRO CON TU DBCONTEXT
             * 
             * // 1. Validar que el usuario o correo no existan previamente
             * var exists = await _context.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email);
             * if (exists)
             * {
             *     return Result.Fail400<AuthResponseDTO>("El nombre de usuario o correo ya está registrado.");
             * }
             * 
             * // 2. Encriptar la contraseña (Password Hash)
             * string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
             * 
             * // 3. Crear la entidad en tu tabla de la base de datos
             * var newUser = new User
             * {
             *     UserName = request.UserName,
             *     Email = request.Email,
             *     PasswordHash = passwordHash,
             *     Role = request.Role ?? "User",
             *     CreatedAt = DateTime.UtcNow
             * };
             * 
             * _context.Users.Add(newUser);
             * await _context.SaveChangesAsync();
             * 
             * // 4. Generar token para el nuevo usuario registrado
             * var tokenResult = GenerateJwtToken(newUser.Id.ToString(), newUser.UserName, newUser.Email, newUser.Role);
             * return Result.Ok(tokenResult, "Usuario registrado exitosamente.");
              */

            // MODO ACTUAL:
            var tokenDemo = GenerateJwtToken(Guid.NewGuid().ToString(), request.UserName, request.Email, request.Role);
            return await Task.FromResult(Result.Ok(tokenDemo, "Usuario registrado exitosamente."));
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
