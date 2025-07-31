using System.Net.Http.Headers;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ProfileController(ISendRequestService requestService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProfileResponse>>> GetProfile(Guid id)
    {
        return await requestService.SendRequestAsync<ApiResponse<ProfileResponse>>(HttpMethod.Get, $"/profile/{id}",
            ServiceType.AuthService);
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<object?>>> UpdateAvatar([FromForm] IFormFile? file,
    [FromForm] Guid userId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(ms);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        content.Add(new StringContent(userId.ToString()), "userId");

        var response = await requestService.SendRequestAsync<AvatarResponse>(HttpMethod.Post, "/profile/avatar", ServiceType.AuthService, content: content);

        if (response.Result != null)
            return response.Result;

        if (response.Value == null)
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "Auth Service returned bad response.");

        return Ok(response.Value.Path);
    }
}