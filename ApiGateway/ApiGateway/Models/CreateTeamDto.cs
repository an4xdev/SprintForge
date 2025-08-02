using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CreateTeamDto
{
    [JsonPropertyName("managerId")] public Guid? ManagerId { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }
}