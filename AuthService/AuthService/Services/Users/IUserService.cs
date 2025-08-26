using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Services.Users;

public interface IUserService
{
    Task<Result<UserResponse>> RegisterAsync(AdminRegisterRequest request);
    Task<Result<List<UserResponse>>> GetAllUsers();
    Task<Result<ProfileResponse?>> GetProfile(Guid id);

    Task<Result<int>> GetCount();

    Task<bool> IsUserInDatabase(Guid id);

    Task<string> UpdateAvatar(Guid id, string path);

    Task<Result<UserResponse?>> GetUser(Guid id);

    Task<Result<List<UserResponse>>> GetAllUsersByRole(string role);
}