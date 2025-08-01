using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class AdminRegisterRequest : UserLoginRequest
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
}