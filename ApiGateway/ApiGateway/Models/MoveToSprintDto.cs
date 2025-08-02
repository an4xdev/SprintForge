using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class MoveToSprintDto
{
    [JsonPropertyName("sprintId")] public Guid SprintId { get; set; }
}