using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class ProjectReportDto
{
    [JsonPropertyName("projectId")] public Guid ProjectID { get; set; }
    [JsonPropertyName("projectName")] public string ProjectName { get; set; }
    [JsonPropertyName("companyName")] public string CompanyName { get; set; }
    [JsonPropertyName("sprintCount")] public int SprintCount { get; set; }
    [JsonPropertyName("taskCount")] public int TaskCount { get; set; }
    [JsonPropertyName("taskCountCompleted")] public int TaskCountCompleted { get; set; }
    [JsonPropertyName("totalTaskTime")] public string TotalTaskTime { get; set; }
    [JsonPropertyName("projectStartDate")] public DateTime ProjectStartDate { get; set; }
    [JsonPropertyName("projectEndDate")] public DateTime ProjectEndDate { get; set; }
    [JsonPropertyName("completedRatio")] public float? CompletedRatio { get; set; }
}