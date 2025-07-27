namespace SharedObjects.DTOs.Responses;

public class ProfileResponse
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? Avatar { get; set; }
}