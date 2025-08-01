using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TaskStatusesController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskStatusDto>>>> GetAllTaskStatuses()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskStatusDto>>>(HttpMethod.Get,
            "/taskStatuses",
            ServiceType.LaravelService);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TaskStatusDto>>> GetTaskStatus(int id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskStatusDto>>(HttpMethod.Get,
            $"/taskStatuses/{id}",
            ServiceType.LaravelService);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<TaskStatusDto>>> CreateTaskStatus(CreateTaskStatusDto dto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskStatusDto>>(HttpMethod.Post, $"/taskStatuses",
            ServiceType.LaravelService, body: dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<TaskStatusDto>>> UpdateTaskStatus(int id, EditTaskStatusDto dto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskStatusDto>>(HttpMethod.Put, $"/taskStatuses/{id}",
            ServiceType.LaravelService, body: dto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTaskStatus(int id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete, $"/taskStatuses/{id}",
            ServiceType.LaravelService);
    }
}