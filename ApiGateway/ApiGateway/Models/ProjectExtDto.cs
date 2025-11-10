using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class ProjectExtDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }

    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }

    [JsonPropertyName("company")] public required CompanyDto Company { get; set; }
}