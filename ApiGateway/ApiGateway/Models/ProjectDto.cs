using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class ProjectDto
{
    [JsonPropertyName("Id")] public Guid Id { get; set; }

    [JsonPropertyName("Name")] public string Name { get; set; }

    [JsonPropertyName("StartDate")] public DateOnly StartDate { get; set; }

    [JsonPropertyName("EndDate")] public DateOnly EndDate { get; set; }

    [JsonPropertyName("CompanyDto")] public CompanyDto CompanyDto { get; set; }
}