using AuthService.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<object?>>> Register(AdminRegisterRequest request)
        {
            var result = await authService.RegisterAsync(request);

            return result.ToActionResult();
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(UserLoginRequest request)
        {
            var result = await authService.LoginAsync(request);
            return result.ToActionResult();
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokensAsync(request);
            return result.ToActionResult();
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object?>>> ChangePassword(ChangePasswordRequest request)
        {
            var result = await authService.ChangePassword(request);
            return result.ToActionResult();
        }
    }
}