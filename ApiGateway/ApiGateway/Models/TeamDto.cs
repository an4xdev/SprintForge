using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TeamDto
{
    [JsonPropertyName("Id")] public string Id { get; set; }

    [JsonPropertyName("Name")] public string Name { get; set; }

    [JsonPropertyName("Manager")] public ManagerDto Manager { get; set; }
}

public class ManagerDto
{
    [JsonPropertyName("Id")] public string Id { get; set; }

    [JsonPropertyName("Username")] public string Username { get; set; }
}