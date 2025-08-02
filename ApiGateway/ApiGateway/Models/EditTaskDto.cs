using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class EditTaskDto
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("taskTypeId")] public int? TaskTypeId { get; set; }
}