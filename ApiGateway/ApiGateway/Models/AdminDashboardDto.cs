using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class AdminDashboardDto
{
    [JsonPropertyName("companiesCount")] public int CompaniesCount { get; set; }
    [JsonPropertyName("usersCount")] public int UsersCount { get; set; }
    [JsonPropertyName("projectsCount")] public long ProjectsCount { get; set; }
    [JsonPropertyName("teamsCount")] public long TeamsCount { get; set; }

}