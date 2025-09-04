using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedObjects.AppDbContext;
using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Models;
using SharedObjects.Responses;
using StackExchange.Redis;

namespace AuthService.Services.Auth;

public class AuthService(AppDbContext context, IDatabase redis) : IAuthService
{
    public async Task<Result<LoginResponse>> LoginAsync(UserLoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash,
                user.PasswordSalt + request.Password)
            == PasswordVerificationResult.Failed)
        {
            return Result<LoginResponse>.BadRequest("Invalid username or password");
        }

        var tokenResponse = await CreateTokenResponse(user);

        var result = new LoginResponse
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            NeedResetPassword = user.NeedResetPassword
        };

        return Result<LoginResponse>.Success(result, "Login Successful");
    }

    private async Task<TokenResponse> CreateTokenResponse(User user)
    {
        var accessToken = CreateToken(user);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);
        
        if (user.Role == "manager")
        {
            await SaveManagerTokenInRedis(user.Id.ToString(), accessToken);
        }
        
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
    
    private async Task<bool> SaveManagerTokenInRedis(string managerId, string accessToken)
    {
        try
        {
            var key = $"manager_token:{managerId}";
            var ok = await redis.StringSetAsync(key, accessToken, TimeSpan.FromDays(1));
            return ok;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save manager token in Redis: {ex.Message}");
        }
        return false;
    }

    public async Task<Result<TokenResponse>> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user is null)
        {
            return Result<TokenResponse>.BadRequest("Invalid refresh token");
        }

        var result = await CreateTokenResponse(user);
        return Result<TokenResponse>.Success(result, "Refresh Tokens Successful");
    }

    public async Task<Result<object?>> ChangePassword(ChangePasswordRequest request)
    {
        var user = await context.Users.Where(u => u.Id == request.UserId).FirstOrDefaultAsync();

        if (user is null)
        {
            return Result<object?>.NotFound("No user found");
        }

        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash,
                user.PasswordSalt + request.OldPassword)
            == PasswordVerificationResult.Failed)
        {
            return Result<object?>.BadRequest("Invalid old password");
        }

        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, user.PasswordSalt + request.NewPassword);

        user.PasswordHash = hashedPassword;

        if (user.NeedResetPassword)
        {
            user.NeedResetPassword = false;
        }

        await context.SaveChangesAsync();

        return Result<object?>.Success(null, "Successfully changed password");
    }

    private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null || user.RefreshToken != refreshToken
                         || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }

        return user;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();
        return refreshToken;
    }

    private static string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_TOKEN")!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}