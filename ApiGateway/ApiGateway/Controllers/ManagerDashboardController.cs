using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "manager")]
public class ManagerDashboardController(
    ISendRequestService sendRequestService)
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ManagerDashboardInfoDto>>> GetManagerDashboardInfo(Guid id)
    {
        var result = new ManagerDashboardInfoDto();

        var activeSprintResult =
            await sendRequestService.SendRequestAsync<ApiResponse<List<ManagerSprintDto>>>(HttpMethod.Get,
                $"/sprints/manager/{id}/active", ServiceType.FastApiService);

        if (!Utils.IsSuccessResult(activeSprintResult))
        {
            return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
        }

        var activeSprintData = ApiResponseExtensions.GetResultDataReference(activeSprintResult);

        if (activeSprintData is null)
        {
            return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
        }

        // perform request for last known sprint
        if (activeSprintData.Count == 0)
        {
            result.IsActive = false;

            var lastSprintResult =
                await sendRequestService.SendRequestAsync<ApiResponse<ManagerSprintDto>>(HttpMethod.Get,
                    $"/sprints/manager/{id}/active/last", ServiceType.FastApiService);

            if (!Utils.IsSuccessResult(lastSprintResult))
            {
                return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
            }

            var lastSprintData = ApiResponseExtensions.GetResultDataReference(lastSprintResult);


            if (lastSprintData is null)
            {
                return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
            }

            activeSprintData = [lastSprintData];
        }
        else
        {
            result.IsActive = true;
        }

        result.Sprints = activeSprintData.ToList();

        result.TasksBySprint = new List<ManagerTasksBySprintDto>(activeSprintData.Count);

        foreach (var sprint in activeSprintData)
        {
            var tasksBySprintResult =
                await sendRequestService.SendRequestAsync<ApiResponse<List<ManagerTaskDto>>>(HttpMethod.Get,
                    $"/sprints/{sprint.SprintId}/tasks/count", ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(tasksBySprintResult))
            {
                return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
            }

            var tasksBySprintData = ApiResponseExtensions.GetResultDataReference(tasksBySprintResult);

            if (tasksBySprintData is null)
            {
                return Result<ManagerDashboardInfoDto>.InternalError("Some unexpected error occurred").ToActionResult();
            }

            result.TasksBySprint.Add(new ManagerTasksBySprintDto
            {
                SprintId = sprint.SprintId,
                Tasks = tasksBySprintData
            });
        }

        return Result<ManagerDashboardInfoDto>
            .Success(result, "Data to manager dashboard has been successfully retrieved").ToActionResult();
    }
}