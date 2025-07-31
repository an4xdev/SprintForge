using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CreateProjectDto
{
    [JsonPropertyName("Name")] public string? Name { get; set; }

    [JsonPropertyName("StartDate")] public DateOnly? StartDate { get; set; }

    [JsonPropertyName("EndDate")] public DateOnly? EndDate { get; set; }

    [JsonPropertyName("CompanyId")] public int? CompanyId { get; set; }
}