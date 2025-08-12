using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class ManagerDashboardInfoDto
{
    [JsonPropertyName("sprints")] public List<ManagerSprintDto> Sprints { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("tasksBySprint")] public List<ManagerTasksBySprintDto> TasksBySprint { get; set; }
}

public class ManagerTasksBySprintDto
{
    [JsonPropertyName("sprintId")] public Guid SprintId { get; set; }
    [JsonPropertyName("tasks")] public List<ManagerTaskDto> Tasks { get; set; }
}

public class ManagerSprintDto
{
    [JsonPropertyName("id")] public Guid SprintId { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }
}

public class ManagerTaskDto
{
    [JsonPropertyName("statusId")] public int StatusId { get; set; }
    [JsonPropertyName("statusName")] public string StatusName { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
}