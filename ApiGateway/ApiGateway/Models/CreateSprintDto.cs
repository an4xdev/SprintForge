using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CreateSprintDto
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }
    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }
    [JsonPropertyName("teamId")] public Guid TeamId { get; set; }
    [JsonPropertyName("managerId")] public Guid ManagerId { get; set; }
    [JsonPropertyName("projectId")] public Guid ProjectId { get; set; }
}