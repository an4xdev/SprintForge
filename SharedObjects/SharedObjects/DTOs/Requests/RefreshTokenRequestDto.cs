using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class RefreshTokenRequestDto
{
    [JsonPropertyName("userId")] public Guid UserId { get; set; }
    [JsonPropertyName("refreshToken")] public required string RefreshToken { get; set; }
}