using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Models;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpPost("register")]
    [Authorize(Roles = "admin", AuthenticationSchemes = "Bearer")]
    public async Task<ActionResult<ApiResponse<User>>> Register(AdminRegisterRequest request)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<User>>(HttpMethod.Post, "/auth/register",
            ServiceType.AuthService, body: request);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> Login(UserLoginRequest request)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TokenResponse>>(HttpMethod.Post, "/auth/login",
            ServiceType.AuthService, body: request);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> RefreshToken(RefreshTokenRequestDto request)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TokenResponse>>(HttpMethod.Post,
            "/auth/refresh-token", ServiceType.AuthService, body: request);
    }
}