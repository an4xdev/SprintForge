using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class SprintReportDto
{
    [JsonPropertyName("sprintId")] public Guid SprintID { get; set; }
    [JsonPropertyName("sprintName")] public string SprintName { get; set; }
    [JsonPropertyName("taskCount")] public int TaskCount { get; set; }
    [JsonPropertyName("taskCountCompleted")] public int TaskCountCompleted { get; set; }
    [JsonPropertyName("totalTaskTime")] public string TotalTaskTime { get; set; } = string.Empty;
    [JsonPropertyName("completedRatio")] public float CompletedRatio { get; set; }
}