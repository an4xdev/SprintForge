using AuthService.Services.File;
using AuthService.Services.Profile;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController(IProfileService profileService, IFileService fileService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProfileResponse?>>> Get(Guid id)
    {
        var profile = await profileService.Get(id);

        return profile.ToActionResult();
    }

    [HttpPost("avatar")]
    public async Task<ActionResult> UpdateAvatar(
        [FromForm] IFormFile file,
        [FromForm] Guid userId)
    {

        if (await profileService.IsUserInDatabase(userId) == false)
        {
            return BadRequest("No user found.");
        }
        if (file.Length == 0)
            return BadRequest("No file uploaded.");

        var ext = Path.GetExtension(file.FileName);
        var blobName = $"{userId}/{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();

        string? resultUrl;
        try
        {
            resultUrl = await fileService.UploadFileAsync(
                objectName: blobName,
                fileStream: stream,
                contentType: file.ContentType);

            if (resultUrl != null)
            {
                await profileService.UpdateAvatar(userId, resultUrl);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Can't upload avatar.");
        }

        return resultUrl == null ?
            StatusCode(502, "Storage service is unavailable.") :
            Ok(new AvatarResponse{ Path = $"{profileService.FileServerPath}/{resultUrl}"});
    }
}