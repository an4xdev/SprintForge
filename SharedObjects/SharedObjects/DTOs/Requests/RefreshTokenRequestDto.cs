using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class RefreshTokenRequestDto
{
    [JsonPropertyName("userId")] public required Guid UserId { get; set; }
    [JsonPropertyName("refreshToken")] public required string RefreshToken { get; set; }
}