using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedObjects.AppDbContext;
using SharedObjects.DTOs.Requests;
using SharedObjects.DTOs.Responses;
using SharedObjects.Models;
using SharedObjects.Responses;
using Task = System.Threading.Tasks.Task;

namespace AuthService.Services.Users;

public class UserService(AppDbContext context, IConfiguration configuration) : IUserService
{
    public async Task<Result<UserResponse>> RegisterAsync(AdminRegisterRequest request)
    {
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return Result<UserResponse>.BadRequest("Username already exists.");
        }

        var salt = GenerateSalt();
        var user = new User
        {
            Username = request.Username,
            PasswordHash = string.Empty,
            PasswordSalt = salt,
            Role = request.Role,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, salt + request.Password);

        user.PasswordHash = hashedPassword;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Avatar = user.Avatar,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };

        return Result<UserResponse>.Success(result, "Registration Successful");
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public async Task<Result<List<UserResponse>>> GetAllUsers()
    {
        var users = await context.Users.Select(u => new UserResponse
        {
            Id = u.Id,
            Username = u.Username,
            Avatar = u.Avatar,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
        }).ToListAsync();
        return Result<List<UserResponse>>.Success(users, "Successfully retrieved all users");
    }

    public async Task<Result<ProfileResponse?>> GetProfile(Guid id)
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
            Username = user.Username
        }, "Successfully retrieved profile");
    }

    public async Task<Result<int>> GetCount()
    {
        var count = await context.Users.CountAsync();
        return Result<int>.Success(count, "Successfully retrieved count of users");
    }

    public Task<bool> IsUserInDatabase(Guid id)
    {
        return Task.FromResult(context.Users.Any(u => u.Id == id));
    }

    public async Task<string> UpdateAvatar(Guid id, string path)
    {
        var user = await context.Users.FirstAsync(u => u.Id == id);

        var avatar = $"{FileServerPath}/{path}";

        user.Avatar = avatar;

        await context.SaveChangesAsync();

        return avatar;
    }

    public async Task<Result<UserResponse?>> GetUser(Guid id)
    {
        if (!await IsUserInDatabase(id))
        {
            return Result<UserResponse?>.NotFound("User not found");
        }
        var user = await context.Users.FindAsync(id);

        return Result<UserResponse?>.Success(new UserResponse
        {
            Id = user!.Id,
            Avatar = user.Avatar,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        }, "Successfully retrieved user");
    }

    private string FileServerPath => $"{configuration["MINIO_PUBLIC_URL"]}/{configuration["MINIO_BUCKET"]}";
}