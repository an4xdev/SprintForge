using Microsoft.EntityFrameworkCore;
using SharedObjects.AppDbContext;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Services.Profile;

public class ProfileService(AppDbContext context) : IProfileService
{
    public async Task<Result<ProfileResponse?>> Get(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return Result<ProfileResponse?>.NotFound("User not found");
        }

        return Result<ProfileResponse?>.Success(new ProfileResponse
        {
            Id = user.Id,
            Avatar = user.Avatar,
            Username = user.Username,
        }, "Successfully retrieved profile");
    }

    public Task<bool> IsUserInDatabase(Guid id)
    {
        return Task.FromResult(context.Users.Any(u => u.Id == id));
    }

    public async Task UpdateAvatar(Guid id, string path)
    {
        var user = await context.Users.FirstAsync(u => u.Id == id);

        user.Avatar = $"{FileServerPath}/{path}";

        await context.SaveChangesAsync();
    }

    public string FileServerPath => "http://minio:9000/uploads/";
}