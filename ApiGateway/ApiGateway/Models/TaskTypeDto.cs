using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TaskTypeDto
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }
}