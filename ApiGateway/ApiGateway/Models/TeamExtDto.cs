using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TeamExtDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("name")] public required string Name { get; set; }
    
    [JsonPropertyName("manager")] public required ManagerExtendedDto Manager { get; set; }
    
    [JsonPropertyName("project")] public required ProjectBasicDto Project { get; set; }
    
    [JsonPropertyName("developers")] public List<DeveloperBasicDto> Developers { get; set; } = new();
}

public class ManagerExtendedDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("username")] public required string Username { get; set; }
    
    [JsonPropertyName("firstName")] public required string FirstName { get; set; }
    
    [JsonPropertyName("lastName")] public required string LastName { get; set; }
    
    [JsonPropertyName("email")] public required string Email { get; set; }
}

public class DeveloperBasicDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("username")] public required string Username { get; set; }
    
    [JsonPropertyName("firstName")] public required string FirstName { get; set; }
    
    [JsonPropertyName("lastName")] public required string LastName { get; set; }
}