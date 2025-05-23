using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedObjects.AppDbContext;
using SharedObjects.DTOs;
using SharedObjects.Models;
using SharedObjects.Responses;

namespace AuthService.Services.Auth;

public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
{
    public async Task<Result<TokenResponseDto>>LoginAsync(UserLoginDto request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash,
                user.PasswordSalt + request.Password)
            == PasswordVerificationResult.Failed)
        {
            return Result<TokenResponseDto>.BadRequest("Invalid username or password");
        }

        var result = await CreateTokenResponse(user);

        return Result<TokenResponseDto>.Success(result, "Login Successful");
    }

    private async Task<TokenResponseDto> CreateTokenResponse(User user)
    {
        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }

    public async Task<Result<User>> RegisterAsync(AdminRegisterDto request)
    {
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return Result<User>.BadRequest("Username already exists.");
        }

        var salt = GenerateSalt();
        var user = new User
        {
            Username = request.Username,
            PasswordHash = string.Empty,
            PasswordSalt = salt,
            Role = request.Role,
        };
        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, salt + request.Password);

        user.PasswordHash = hashedPassword;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Result<User>.Success(user, "Registration Successful");
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public async Task<Result<TokenResponseDto>> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user is null)
        {
            return Result<TokenResponseDto>.BadRequest("Invalid refresh token");
        }

        var result = await CreateTokenResponse(user);
        return Result<TokenResponseDto>.Success(result, "Refresh Tokens Successful");
    }

    public async Task<Result<object?>> ChangePassword(ChangePasswordDto request)
    {
        var user = await context.Users.Where(u=>u.Id == request.UserId).FirstOrDefaultAsync();

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

        await context.SaveChangesAsync();

        return Result<object?>.Success(null,"Successfully changed password");
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

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetValue<string>("Jwt:Token")!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("Jwt:Issuer"),
            audience: configuration.GetValue<string>("Jwt:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}