using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Requests;

public class ChangePasswordRequest
{
    [JsonPropertyName("userId")] public Guid UserId { get; set; }

    [JsonPropertyName("oldPassword")] public string OldPassword { get; set; } = string.Empty;

    [JsonPropertyName("newPassword")] public string NewPassword { get; set; } = string.Empty;
}