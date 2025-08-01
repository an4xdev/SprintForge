using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CompanyDto
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }
}