using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class ChangePasswordRequest
{
    [JsonPropertyName("userId")] public required Guid UserId { get; set; }

    [JsonPropertyName("oldPassword")] public required string OldPassword { get; set; } = string.Empty;

    [JsonPropertyName("newPassword")] public required string NewPassword { get; set; } = string.Empty;
}