using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class TaskHistoryDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("task_id")] public Guid TaskId { get; set; }
    [JsonPropertyName("change_date")] public DateTime ChangeDate { get; set; }
    [JsonPropertyName("old_status")] public string? OldStatus { get; set; }
    [JsonPropertyName("new_status")] public string NewStatus { get; set; }
}