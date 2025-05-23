using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs;
using SharedObjects.Models;
using SharedObjects.Responses;

namespace AuthService.Services.Auth;

public interface IAuthService
{
    Task<Result<User>> RegisterAsync(AdminRegisterDto request);
    Task<Result<TokenResponseDto>> LoginAsync(UserLoginDto request);
    Task<Result<TokenResponseDto>> RefreshTokensAsync(RefreshTokenRequestDto request);

    Task<Result<object?>> ChangePassword(ChangePasswordDto request);
}