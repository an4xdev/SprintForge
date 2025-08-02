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
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetTasks()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(HttpMethod.Get,
            "/tasks",
            ServiceType.LaravelService);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTask(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Get,
            $"/tasks/{id}",
            ServiceType.LaravelService);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskDto>>> PostTask(CreateTaskDto createTaskDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Post,
            "/tasks",
            ServiceType.LaravelService, body: createTaskDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> PutTask(Guid id, EditTaskDto editTaskDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Put,
            $"/tasks/{id}",
            ServiceType.LaravelService, body: editTaskDto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTask(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete,
            $"/tasks/{id}",
            ServiceType.LaravelService);
    }

    [HttpPatch("{id:guid}/assign-developer")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> AssignDeveloper(Guid id,
        AssignDeveloperDto assignDeveloperDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Patch,
            $"/tasks/{id}/assign-developer",
            ServiceType.LaravelService, body: assignDeveloperDto);
    }

    [HttpPatch("{id:guid}/move-to-sprint")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> MoveToSprint(Guid id, MoveToSprintDto moveToSprintDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Patch,
            $"/tasks/{id}/move-to-sprint",
            ServiceType.LaravelService, body: moveToSprintDto);
    }

    private readonly List<string> _actions = ["start", "pause", "stop"];

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