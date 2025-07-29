using System.Text.Json.Serialization;

namespace SharedObjects.Responses;

public class ApiResponse<T>
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}