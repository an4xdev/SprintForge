using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TaskActionResponseDto
{
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("taskId")] public Guid TaskId { get; set; }
}