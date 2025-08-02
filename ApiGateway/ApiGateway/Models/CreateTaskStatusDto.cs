using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CreateTaskStatusDto
{
    [JsonPropertyName("name")] public string Name { get; set; }
}