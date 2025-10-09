using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class AuditLogsDto
{
    [JsonPropertyName("logs")] public List<AuditLogDto> Logs { get; set; }
    [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
    [JsonPropertyName("limit")] public int Limit { get; set; }
    [JsonPropertyName("offset")] public int Offset { get; set; }
}

public class AuditLogDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
    [JsonPropertyName("service")] public string Service { get; set; }
    [JsonPropertyName("action")] public string Action { get; set; }
    [JsonPropertyName("entity")] public string Entity { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
}