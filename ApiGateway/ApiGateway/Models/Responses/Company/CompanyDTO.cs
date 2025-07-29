using System.Text.Json.Serialization;

namespace ApiGateway.Models.Responses.Company;

public class CompanyDto
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; }
}