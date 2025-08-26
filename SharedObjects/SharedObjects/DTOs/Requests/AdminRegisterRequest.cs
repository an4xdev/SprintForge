using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class AdminRegisterRequest : UserLoginRequest
{
    [JsonPropertyName("role")] public required string Role { get; set; } = string.Empty;
    [JsonPropertyName("email")] public required string Email { get; set; } = string.Empty;
    [JsonPropertyName("firstName")] public required string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("lastName")] public required string LastName { get; set; } = string.Empty;
}