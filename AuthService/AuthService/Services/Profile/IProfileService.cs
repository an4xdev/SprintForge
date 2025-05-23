using SharedObjects.DTOs;

namespace AuthService.Services.Profile;

public interface IProfileService
{
    Task<ProfileDto?> Get(Guid id);

    Task<bool> IsUserInDatabase(Guid id);

    Task UpdateAvatar(Guid id, string path);

    string FileServerPath { get; }
}