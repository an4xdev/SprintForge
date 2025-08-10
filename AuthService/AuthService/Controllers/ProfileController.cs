using AuthService.Services.File;
using AuthService.Services.Profile;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(IProfileService profileService, IFileService fileService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProfileResponse?>>> Get(Guid id)
    {
        var profile = await profileService.Get(id);

        return profile.ToActionResult();
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<AvatarResponse>>> UpdateAvatar(
        [FromForm] IFormFile file,
        [FromForm] Guid userId)
    {
        if (await profileService.IsUserInDatabase(userId) == false)
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
                fullPath = await profileService.UpdateAvatar(userId, resultUrl);
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