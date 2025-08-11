using AuthService.Services.File;
using AuthService.Services.Users;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService, IFileService fileService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register(AdminRegisterRequest request)
    {
        var result = await userService.RegisterAsync(request);

        return result.ToActionResult();
    }
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetAllUsers()
    {
        var users = await userService.GetAllUsers();
        return users.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse?>>> Get(Guid id)
    {
        var profile = await userService.Get(id);

        return profile.ToActionResult();
    }

    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<int>>> GetCount()
    {
        var count = await userService.GetCount();

        return count.ToActionResult();
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<AvatarResponse>>> UpdateAvatar(
        [FromForm] IFormFile file,
        [FromForm] Guid userId)
    {
        if (await userService.IsUserInDatabase(userId) == false)
        {
            return Result<AvatarResponse>.BadRequest("User not found.").ToActionResult();
        }

        if (file.Length == 0)
        {
            return Result<AvatarResponse>.BadRequest("No file uploaded.").ToActionResult();
        }

        var ext = Path.GetExtension(file.FileName);
        var blobName = $"{userId}/{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();
        string? fullPath = null;
        try
        {
            var resultUrl = await fileService.UploadFileAsync(
                objectName: blobName,
                fileStream: stream,
                contentType: file.ContentType);
            if (resultUrl != null)
            {
                fullPath = await userService.UpdateAvatar(userId, resultUrl);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Result<AvatarResponse>.InternalError("Can't upload avatar.").ToActionResult();
        }

        return fullPath == null
            ? Result<AvatarResponse>.InternalError("Storage service is unavailable.").ToActionResult()
            : Result<AvatarResponse>.Success(new AvatarResponse { Path = fullPath }, "Successfully updated avatar.")
                .ToActionResult();
    }
}