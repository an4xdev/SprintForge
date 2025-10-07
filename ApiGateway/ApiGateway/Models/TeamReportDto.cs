using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TeamReportDto
{
    [JsonPropertyName("developerCount")] public int DeveloperCount { get; set; }
    [JsonPropertyName("developerIds")] public List<Guid> DeveloperIds { get; set; }
    [JsonPropertyName("sprintsNames")] public List<string> SprintsNames { get; set; }
    [JsonPropertyName("taskCount")] public int TaskCount { get; set; }
    [JsonPropertyName("taskCountCompleted")] public int TaskCountCompleted { get; set; }
    [JsonPropertyName("totalTaskTime")] public string TotalTaskTime { get; set; }
}