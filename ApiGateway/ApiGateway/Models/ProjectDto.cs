using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class ProjectDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("startDate")] public DateOnly StartDate { get; set; }

    [JsonPropertyName("endDate")] public DateOnly EndDate { get; set; }

    [JsonPropertyName("companyDto")] public CompanyDto CompanyDto { get; set; }
}