using System.Net.Http.Headers;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UsersController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetAllUsers()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<UserResponse>>>(HttpMethod.Get, "/users",
            ServiceType.AuthService);
    }

    [HttpPost("register")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register(AdminRegisterRequest request)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<UserResponse>>(HttpMethod.Post, "/users",
            ServiceType.AuthService, body: request);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<UserResponse>>(HttpMethod.Get, $"/users/{id}",
            ServiceType.AuthService);
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<AvatarResponse>>> UpdateAvatar([FromForm] IFormFile? file,
        [FromForm] Guid userId)
    {
        if (file == null || file.Length == 0)
        {
            return Result<AvatarResponse>.BadRequest("No file uploaded.").ToActionResult();
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(ms);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        content.Add(new StringContent(userId.ToString()), "userId");

        var response = await sendRequestService.SendRequestAsync<ApiResponse<AvatarResponse>>(HttpMethod.Post,
            "/users/avatar",
            ServiceType.AuthService, content: content);

        await sendRequestService.InvalidateCacheAsync($"/users/{userId}", ServiceType.AuthService);

        return response;
    }
}