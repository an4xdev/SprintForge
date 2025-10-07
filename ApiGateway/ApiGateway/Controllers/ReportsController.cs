using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ReportsController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet("sprints")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<SprintReportDto>>>> GetSprintsReports(
        Guid? managerId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var queryParameters = new List<string>();

        if (managerId.HasValue)
            queryParameters.Add($"managerId={managerId.Value}");
        if (startDate.HasValue)
            queryParameters.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParameters.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var queryString = queryParameters.Count != 0 ? "?" + string.Join("&", queryParameters) : string.Empty;

        var url = $"/reports/sprints{queryString}";

        return await sendRequestService.SendRequestAsync<ApiResponse<List<SprintReportDto>>>(
            HttpMethod.Get,
            url,
            ServiceType.ReportsService
        );
    }

    [HttpGet("teams")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<TeamReportDto>>>> GetTeamsReports(
        Guid? managerId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var queryParameters = new List<string>();

        if (managerId.HasValue)
            queryParameters.Add($"managerId={managerId.Value}");
        if (startDate.HasValue)
            queryParameters.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParameters.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var queryString = queryParameters.Count != 0 ? "?" + string.Join("&", queryParameters) : string.Empty;

        var url = $"/reports/teams{queryString}";

        return await sendRequestService.SendRequestAsync<ApiResponse<List<TeamReportDto>>>(
            HttpMethod.Get,
            url,
            ServiceType.ReportsService
        );
    }

    [HttpGet("projects")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<ProjectReportDto>>>> GetProjectsReports(
        Guid? managerId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var queryParameters = new List<string>();

        if (managerId.HasValue)
            queryParameters.Add($"managerId={managerId.Value}");
        if (startDate.HasValue)
            queryParameters.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue)
            queryParameters.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        var queryString = queryParameters.Count != 0 ? "?" + string.Join("&", queryParameters) : string.Empty;

        var url = $"/reports/projects{queryString}";

        return await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectReportDto>>>(
            HttpMethod.Get,
            url,
            ServiceType.ReportsService
        );
    }
}