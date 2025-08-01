using System.Text.Json.Serialization;

namespace SharedObjects.DTOs.Responses;

public class LoginResponse : TokenResponse
{
    [JsonPropertyName("needResetPassword")] public bool NeedResetPassword { get; set; }
}