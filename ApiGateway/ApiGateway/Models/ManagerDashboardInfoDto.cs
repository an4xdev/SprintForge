using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class ManagerDashboardInfoDto
{
    [JsonPropertyName("sprint")] public ManagerSprintDto? Sprint { get; set; }
    [JsonPropertyName("tasks")] public List<ManagerTaskDto> Tasks { get; set; } = [];
}

public class ManagerSprintDto
{
    [JsonPropertyName("id")] public Guid SprintId { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("active")] public bool IsActive { get; set; }

    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }

    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }
}

public class ManagerTaskDto
{
    [JsonPropertyName("statusId")] public int StatusId { get; set; }
    [JsonPropertyName("statusName")] public string StatusName { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}