using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CreateTeamDto
{
    [JsonPropertyName("ManagerId")]
    public Guid? ManagerId { get; set; }
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}