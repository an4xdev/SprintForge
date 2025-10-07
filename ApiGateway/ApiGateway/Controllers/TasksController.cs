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
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetAllTasks()
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
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(CreateTaskDto createTaskDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Post,
            "/tasks",
            ServiceType.LaravelService, body: createTaskDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(Guid id, EditTaskDto editTaskDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Put,
            $"/tasks/{id}",
            ServiceType.LaravelService, body: editTaskDto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTask(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete,
            $"/tasks/{id}",
            ServiceType.LaravelService);
    }

    [HttpPatch("{id:guid}/assign-developer")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> AssignDeveloper(Guid id,
        AssignDeveloperDto assignDeveloperDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Patch,
            $"/tasks/{id}/assign-developer",
            ServiceType.LaravelService, body: assignDeveloperDto);
    }

    [HttpPatch("{id:guid}/move-to-sprint")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> MoveToSprint(Guid id, MoveToSprintDto moveToSprintDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(HttpMethod.Patch,
            $"/tasks/{id}/move-to-sprint",
            ServiceType.LaravelService, body: moveToSprintDto);
    }

    private readonly List<string> _actions = ["start", "pause", "stop"];

    [Authorize(Roles = "admin,developer")]
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

    [Authorize(Roles = "admin,manager")]
    [HttpGet("unassigned/project/{id:guid}")]
    public async Task<ActionResult<ApiResponse<List<TaskDto>?>>> GetUnassignedTasksByProject(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>?>>(HttpMethod.Get,
            $"/tasks/unassigned/project/{id}", ServiceType.LaravelService);
    }

    [Authorize(Roles = "admin,manager")]
    [HttpGet("unassigned/sprint/{id:guid}")]
    public async Task<ActionResult<ApiResponse<List<TaskDto>?>>> GetUnassignedTasksBySprint(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>?>>(HttpMethod.Get, $"/tasks/unassigned/sprint/{id}", ServiceType.LaravelService);
    }

    [Authorize(Roles = "admin,manager")]
    [HttpGet("assigned/sprint/{id:guid}")]
    public async Task<ActionResult<ApiResponse<List<TaskDto>?>>> GetAssignedTasksBySprint(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>?>>(HttpMethod.Get,
            $"/tasks/assigned/sprint/{id}", ServiceType.LaravelService);
    }

    [Authorize(Roles = "admin,developer")]
    [HttpGet("developer/{id:guid}")]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetTasksByDeveloper(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(HttpMethod.Get, $"/tasks/developer/{id}", ServiceType.LaravelService);
    }

    [Authorize(Roles = "admin,manager")]
    [HttpGet("sprint/{id:guid}/count")]
    public async Task<ActionResult<ApiResponse<List<ManagerTaskDto>>>> GetTasksBySprintCount(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<ManagerTaskDto>>>(HttpMethod.Get,
            $"/tasks/sprint/{id}/count", ServiceType.LaravelService);
    }

    [Authorize(Roles = "admin,developer")]
    [HttpGet("{id:guid}/time")]
    public async Task<ActionResult<ApiResponse<TaskTimeDto>>> GetTaskTime(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskTimeDto>>(HttpMethod.Get,
            $"/tasks/{id}/time", ServiceType.FastApiService);
    }

    [Authorize(Roles = "admin,developer")]
    [HttpGet("developer/{id:guid}/times")]
    public async Task<ActionResult<ApiResponse<List<TaskTimeDto>>>> GetDeveloperTaskTimes(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskTimeDto>>>(HttpMethod.Get,
            $"/tasks/developer/{id}/times", ServiceType.FastApiService);
    }
}