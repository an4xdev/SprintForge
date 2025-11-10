using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class SprintExtDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("name")] public required string Name { get; set; }
    
    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }
    
    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }
    
    [JsonPropertyName("manager")] public required ManagerBasicDto Manager { get; set; }
    
    [JsonPropertyName("team")] public required TeamBasicDto Team { get; set; }
    
    [JsonPropertyName("project")] public required ProjectBasicDto Project { get; set; }
}

public class ManagerBasicDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("username")] public required string Username { get; set; }
    
    [JsonPropertyName("firstName")] public required string FirstName { get; set; }
    
    [JsonPropertyName("lastName")] public required string LastName { get; set; }
}

public class TeamBasicDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("name")] public required string Name { get; set; }
}

public class ProjectBasicDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("name")] public required string Name { get; set; }
    
    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }
    
    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }
}