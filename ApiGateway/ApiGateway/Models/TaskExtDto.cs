using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TaskExtDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("name")] public required string Name { get; set; }
    
    [JsonPropertyName("description")] public string? Description { get; set; }
    
    [JsonPropertyName("taskType")] public required TaskTypeDto TaskType { get; set; }
    
    [JsonPropertyName("taskStatus")] public required TaskStatusDto TaskStatus { get; set; }
    
    [JsonPropertyName("developer")] public DeveloperBasicDto? Developer { get; set; }
    
    [JsonPropertyName("sprint")] public SprintBasicDto? Sprint { get; set; }
    
    [JsonPropertyName("project")] public ProjectBasicDto? Project { get; set; }
}

public class SprintBasicDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("name")] public required string Name { get; set; }
    
    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }
    
    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }
}