using SharedObjects.DTOs;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Services.Profile;

public interface IProfileService
{
    Task<Result<ProfileResponse?>> Get(Guid id);

    Task<bool> IsUserInDatabase(Guid id);

    Task<string> UpdateAvatar(Guid id, string path);

    string FileServerPath { get; }
}