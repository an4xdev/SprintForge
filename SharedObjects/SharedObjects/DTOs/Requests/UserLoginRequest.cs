using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class UserLoginRequest
{
    [JsonPropertyName("username")] public required string Username { get; set; } = string.Empty;
    [JsonPropertyName("password")] public required string Password { get; set; } = string.Empty;
}