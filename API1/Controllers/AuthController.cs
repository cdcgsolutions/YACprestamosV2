using API1.Common;
using API1.DTOs;
using API1.RN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthRN _authRN;

        public AuthController(AuthRN authRN)
        {
            _authRN = authRN;
        }

        /// <summary>
        /// Inicia sesión y genera un token JWT.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(Result<AuthResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<AuthResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request) =>
            (await _authRN.LoginAsync(request)).ToResult();

        /// <summary>
        /// Registra un nuevo usuario en la aplicación.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Result<AuthResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request) =>
            (await _authRN.RegisterAsync(request)).ToResult();

        /// <summary>
        /// Obtiene el perfil del usuario autenticado actual a partir del token JWT.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(Result<UserProfileDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UserProfileDTO>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile() =>
            (await _authRN.GetProfileAsync(User)).ToResult();
    }
}
