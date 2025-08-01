using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TasksController(ISendRequestService sendRequestService) : ControllerBase
{
    private readonly List<string> _actions = ["start", "pause", "stop"];

    // TODO: implement
    // TODO: split by manager(managing tasks) and developer(start/stop)
    [Authorize(Roles = "developer")]
    [HttpPut("{id:guid}/{command}")]
    public async Task<ActionResult<ApiResponse<TaskActionResponseDto>>> ActionOnTask(Guid id, string command)
    {
        if (!_actions.Contains(command))
        {
            return Result<TaskActionResponseDto>.BadRequest("Unknown command").ToActionResult();
        }

        return await sendRequestService.SendRequestAsync<ApiResponse<TaskActionResponseDto>>(HttpMethod.Put,
            $"/tasks/{id}/{command}",
            ServiceType.FastApiService);
    }
}