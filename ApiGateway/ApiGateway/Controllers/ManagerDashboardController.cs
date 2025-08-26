using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "admin,manager")]
public class ManagerDashboardController(
    ISendRequestService sendRequestService)
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ManagerDashboardInfoDto>>> GetManagerDashboardInfo(Guid id)
    {
        var result = new ManagerDashboardInfoDto();

        var sprintResult =
            await sendRequestService.SendRequestAsync<ApiResponse<ManagerSprintDto>>(HttpMethod.Get,
                $"/sprints/manager/{id}/dashboard", ServiceType.FastApiService);

        if (!Utils.IsSuccessResult(sprintResult))
        {
            return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
        }

        var sprintData = ApiResponseExtensions.GetResultDataReference(sprintResult);

        if (sprintData is null)
        {
            return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
        }

        result.Sprint = sprintData;

        var tasksBySprintResult =
            await sendRequestService.SendRequestAsync<ApiResponse<List<ManagerTaskDto>>>(HttpMethod.Get,
                $"/tasks/sprint/{sprintData.SprintId}/count", ServiceType.LaravelService);

        if (!Utils.IsSuccessResult(tasksBySprintResult))
        {
            return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
        }

        var tasksBySprintData = ApiResponseExtensions.GetResultDataReference(tasksBySprintResult);

        if (tasksBySprintData is not null)
        {
            result.Tasks = tasksBySprintData;
        }
        else
        {
            return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
        }

        return Result<ManagerDashboardInfoDto>
            .Success(result, "Data to manager dashboard has been successfully retrieved").ToActionResult();
    }
}