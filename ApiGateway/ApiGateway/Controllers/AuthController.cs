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
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "admin")]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<User>>> Register(AdminRegisterRequest request)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<User>>(HttpMethod.Post, "/auth/register", ServiceType.AuthService, body:request);
        // try
        // {
        //     var client = httpClientFactory.CreateClient();
        //
        //     var response = await client.PostAsJsonAsync("http://auth/auth/register", request);
        //
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         var errorContent = await response.Content.ReadAsStringAsync();
        //         return StatusCode((int)response.StatusCode, errorContent);
        //     }
        //
        //     var user = await response.Content.ReadFromJsonAsync<User>();
        //
        //     return user is null
        //         ? StatusCode(500, "Failed to deserialize user object from AuthService.") :
        //         Ok(user);
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine(ex.Message);
        //     return StatusCode(500, $"An error occurred: {ex.Message}");
        // }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> Login(UserLoginRequest request)
    {

        return await sendRequestService.SendRequestAsync<ApiResponse<TokenResponse>>(HttpMethod.Post, "/auth/login", ServiceType.AuthService, body:request);
        // try
        // {
        //     var client = httpClientFactory.CreateClient();
        //
        //     var response = await client.PostAsJsonAsync("http://auth/auth/login", request);
        //
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         var errorContent = await response.Content.ReadAsStringAsync();
        //         return StatusCode((int)response.StatusCode, errorContent);
        //     }
        //
        //     var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        //
        //     return token is null
        //         ? StatusCode(500, "Failed to deserialize user object from AuthService.") :
        //         Ok(token);
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine(ex.Message);
        //     return StatusCode(500, $"An error occurred: {ex.Message}");
        // }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<TokenResponse>>> RefreshToken(RefreshTokenRequestDto request)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TokenResponse>>(HttpMethod.Post, "/auth/refresh-token", ServiceType.AuthService, body:request);
        // try
        // {
        //     var client = httpClientFactory.CreateClient();
        //
        //     var response = await client.PostAsJsonAsync("htstp://auth/auth/refresh-token", request);
        //
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         var errorContent = await response.Content.ReadAsStringAsync();
        //         return StatusCode((int)response.StatusCode, errorContent);
        //     }
        //
        //     var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        //
        //     return token is null
        //         ? StatusCode(500, "Failed to deserialize user object from AuthService.") :
        //         Ok(token);
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine(ex.Message);
        //     return StatusCode(500, $"An error occurred: {ex.Message}");
        // }
    }
}