using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class CompanyDto
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }
}