using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TeamDto
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("manager")] public ManagerDto Manager { get; set; }
}

public class ManagerDto
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("username")] public string Username { get; set; }
}