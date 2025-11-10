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

public class UserService(AppDbContext context, IConfiguration configuration, IAuditService auditService) : IUserService
{
    public async Task<Result<UserResponse>> RegisterAsync(AdminRegisterRequest request)
    {
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
        {
            await auditService.SendAuditLogAsync("CREATE_FAILED", "User", $"Failed to create user with username {request.Username}: username already exists.");
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

        await auditService.SendAuditLogAsync("CREATE_SUCCESS", "User", $"{user.Username} created.");

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

        await auditService.SendAuditLogAsync("UPDATE_SUCCESS", "User", $"{user.Username} updated their avatar.");

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

    public async Task<Result<List<UserResponse>>> GetAllUsersByRole(string role)
    {
        List<string> roles = ["admin", "manager", "developer"];
        if (!roles.Contains(role))
        {
            return Result<List<UserResponse>>.NotFound("Unknown role");
        }

        var users = await context.Users
            .Where(u => u.Role == role)
            .Select(a => new UserResponse
            {
                Id = a.Id,
                Username = a.Username,
                Avatar = a.Avatar,
                Email = a.Email,
                FirstName = a.FirstName,
                LastName = a.LastName
            }).ToListAsync();


        return Result<List<UserResponse>>.Success(users, $"Successfully retrieved all users by role: {role}");
    }

    public async Task<Result<UserResponse?>> AdminUpdateUser(Guid id, AdminUpdateUserRequest request)
    {
        if (!await IsUserInDatabase(id))
        {
            await auditService.SendAuditLogAsync("UPDATE_FAILED", "User", $"Failed to update user with ID {id}: user not found.");
            return Result<UserResponse?>.NotFound("User not found");
        }

        List<string> roles = ["admin", "manager", "developer"];
        var user = await context.Users.FindAsync(id);

        if (request.Username != null)
        {
            user!.Username = request.Username;
        }

        if (request.FirstName != null)
        {
            user!.FirstName = request.FirstName;
        }

        if (request.LastName != null)
        {
            user!.LastName = request.LastName;
        }

        if (request.Email != null)
        {
            user!.Email = request.Email;
        }

        if (request.Role != null)
        {
            if (roles.Contains(request.Role))
            {
                user!.Role = request.Role;
            }
            else
            {
                await auditService.SendAuditLogAsync("UPDATE_FAILED", "User", $"Failed to update user with ID {id}: invalid role {request.Role}.");
                return Result<UserResponse?>.BadRequest("Invalid role");
            }
        }

        await context.SaveChangesAsync();

        await auditService.SendAuditLogAsync("UPDATE_SUCCESS", "User", $"{user!.Username} was updated.");

        return Result<UserResponse?>.Success(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Avatar = user.Avatar,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        }, "Successfully updated the user");
    }

    public async Task<Result<UserResponse?>> UpdateUser(Guid id, UpdateUserRequest request)
    {
        if (!await IsUserInDatabase(id))
        {
            await auditService.SendAuditLogAsync("UPDATE_FAILED", "User", $"Failed to update user with ID {id}: user not found.");
            return Result<UserResponse?>.NotFound("User not found");
        }

        var user = await context.Users.FindAsync(id);

        if (request.Username != null)
        {
            user!.Username = request.Username;
        }

        if (request.FirstName != null)
        {
            user!.FirstName = request.FirstName;
        }

        if (request.LastName != null)
        {
            user!.LastName = request.LastName;
        }

        if (request.Email != null)
        {
            user!.Email = request.Email;
        }

        await context.SaveChangesAsync();

        await auditService.SendAuditLogAsync("UPDATE_SUCCESS", "User", $"{user!.Username} was updated.");

        return Result<UserResponse?>.Success(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Avatar = user.Avatar,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        }, "Successfully updated the user");
    }

    public async Task<Result<object?>> DeleteUser(Guid id)
    {
        if (!await IsUserInDatabase(id))
        {
            await auditService.SendAuditLogAsync("DELETE_FAILED", "User", $"Failed to delete user with ID {id}: user not found.");
            return Result<object?>.NotFound("User not found");
        }

        var user = await context.Users.FindAsync(id);

        context.Users.Remove(user!);
        await context.SaveChangesAsync();

        await auditService.SendAuditLogAsync("DELETE_SUCCESS", "User", $"{user!.Username} was deleted.");

        return Result<object?>.NoContent();
    }

    public async Task<Result<List<UserResponse>>> GetDevelopersByTeamId(Guid teamId)
    {
        var team = await context
            .Teams
            .Include(t => t.Developers)
            .FirstOrDefaultAsync(t => t.Id == teamId);
        if (team == null)
        {
            return Result<List<UserResponse>>.NotFound("Team not found");
        }

        var users = team.Developers.Select(d => new UserResponse
        {
            Id = d.Id,
            Username = d.Username,
            Avatar = d.Avatar,
            Email = d.Email,
            FirstName = d.FirstName,
            LastName = d.LastName
        }).ToList();

        return Result<List<UserResponse>>.Success(users, $"Successfully retrieved all users by team: {teamId}");
    }

    private string FileServerPath => $"{configuration["MINIO_PUBLIC_URL"]}/{configuration["MINIO_BUCKET"]}";
}