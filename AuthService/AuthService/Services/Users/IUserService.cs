using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Services.Users;

public interface IUserService
{
    Task<Result<UserResponse>> RegisterAsync(AdminRegisterRequest request);
    Task<Result<List<UserResponse>>> GetAllUsers();
    Task<Result<UserResponse?>> Get(Guid id);

    Task<bool> IsUserInDatabase(Guid id);

    Task<string> UpdateAvatar(Guid id, string path);
}