namespace SharedObjects.DTOs;

public class ProfileDto
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? Avatar { get; set; }
}