using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Responses;

public class AvatarResponse
{
    [JsonPropertyName("path")] public string Path { get; set; }
}