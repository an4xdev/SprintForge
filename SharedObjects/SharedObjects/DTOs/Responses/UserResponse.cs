using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Responses;

public class UserResponse
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("avatar")] public string? Avatar { get; set; }
}