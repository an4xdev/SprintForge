using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CreateTaskDto
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("developerId")] public Guid? DeveloperId { get; set; }
    [JsonPropertyName("sprintId")] public Guid? SprintId { get; set; }
    [JsonPropertyName("taskTypeId")] public int? TaskTypeId { get; set; }
}