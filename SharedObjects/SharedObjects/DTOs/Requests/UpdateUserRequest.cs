using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class UpdateUserRequest
{
    [JsonPropertyName("username")] public string? Username { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string? Email { get; set; } = string.Empty;
    [JsonPropertyName("firstName")] public string? FirstName { get; set; } = string.Empty;
    [JsonPropertyName("lastName")] public string? LastName { get; set; } = string.Empty;
}