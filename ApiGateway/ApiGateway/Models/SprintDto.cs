using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class SprintDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }
    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }
    [JsonPropertyName("teamId")] public Guid TeamId { get; set; }
    [JsonPropertyName("managerId")] public Guid ManagerId { get; set; }
    [JsonPropertyName("projectId")] public Guid? ProjectId { get; set; }
}