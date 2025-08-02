using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class EditTaskStatusDto
{
    [JsonPropertyName("name")] public string Name { get; set; }
}