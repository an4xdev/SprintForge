using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace AuthService.Services.Auth;

public interface IAuthService
{

    Task<Result<LoginResponse>> LoginAsync(UserLoginRequest request);
    Task<Result<TokenResponse>> RefreshTokensAsync(RefreshTokenRequestDto request);

    Task<Result<object?>> ChangePassword(ChangePasswordRequest request);
}