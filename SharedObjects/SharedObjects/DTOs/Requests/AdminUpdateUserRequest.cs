using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class AdminUpdateUserRequest : UpdateUserRequest
{
    [JsonPropertyName("role")] public string? Role { get; set; } = string.Empty;
}