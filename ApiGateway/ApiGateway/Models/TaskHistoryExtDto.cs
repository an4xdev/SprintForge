using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TaskHistoryExtDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("task")] public TaskBasicDto Task { get; set; }
    
    [JsonPropertyName("change_date")] public DateTime ChangeDate { get; set; }
    
    [JsonPropertyName("old_status")] public TaskStatusDto? OldStatus { get; set; }
    
    [JsonPropertyName("new_status")] public TaskStatusDto NewStatus { get; set; }
    
}

public class TaskBasicDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    
    [JsonPropertyName("name")] public required string Name { get; set; }
    
    [JsonPropertyName("description")] public string? Description { get; set; }
}
