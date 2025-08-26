using System.ComponentModel.DataAnnotations;

namespace SharedObjects.Models;

public class User
{
    [Key] public Guid Id { get; set; }
    public required string Username { get; set; } = string.Empty;
    public required string PasswordHash { get; set; } = string.Empty;
    public required string PasswordSalt { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public bool NeedResetPassword { get; set; }

    public string? Avatar { get; set; }

    public required string Email { get; set; } = string.Empty;

    public required string FirstName { get; set; } = string.Empty;
    public required string LastName { get; set; } = string.Empty;
}