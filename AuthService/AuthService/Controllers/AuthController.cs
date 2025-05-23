using SharedObjects.DTOs;
using AuthService.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Models;
using SharedObjects.Responses;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<User>>> Register(AdminRegisterDto request)
        {
            var result = await authService.RegisterAsync(request);

            return result.ToActionResult();
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login(UserLoginDto request)
        {
            var result = await authService.LoginAsync(request);
            return result.ToActionResult();
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokensAsync(request);
            return result.ToActionResult();
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object?>>> ChangePassword(ChangePasswordDto request)
        {
            var result = await authService.ChangePassword(request);
            return result.ToActionResult();
        }
    }
}