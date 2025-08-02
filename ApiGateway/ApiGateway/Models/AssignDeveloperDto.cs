using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class AssignDeveloperDto
{
    [JsonPropertyName("developerId")] public Guid DeveloperId { get; set; }
}