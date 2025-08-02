using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TaskDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("developerId")] public Guid? DeveloperId { get; set; }
    [JsonPropertyName("sprintId")] public Guid? SprintId { get; set; }
    [JsonPropertyName("taskStatusId")] public int TaskStatusId { get; set; }
    [JsonPropertyName("taskTypeId")] public int TaskTypeId { get; set; }
}