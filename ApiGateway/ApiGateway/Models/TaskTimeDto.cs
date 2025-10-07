using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TaskTimeDto
{
    [JsonPropertyName("taskId")] public string TaskId { get; set; } = string.Empty;
    [JsonPropertyName("taskName")] public string? TaskName { get; set; }
    [JsonPropertyName("totalSeconds")] public int TotalSeconds { get; set; }
    [JsonPropertyName("isRunning")] public bool IsRunning { get; set; }
    [JsonPropertyName("currentStatus")] public string CurrentStatus { get; set; } = string.Empty;
    [JsonPropertyName("currentSessionStart")] public string? CurrentSessionStart { get; set; }
}