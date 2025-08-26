using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Responses;

public class UserResponse : ProfileResponse
{
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("firstName")] public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("lastName")] public string LastName { get; set; } = string.Empty;
}