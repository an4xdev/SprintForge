using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CreateProjectDto
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("startDate")] public DateOnly? StartDate { get; set; }

    [JsonPropertyName("endDate")] public DateOnly? EndDate { get; set; }

    [JsonPropertyName("companyId")] public int? CompanyId { get; set; }
}