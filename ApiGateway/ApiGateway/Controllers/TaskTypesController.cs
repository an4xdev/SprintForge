using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TaskTypesController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskTypeDto>>>> GetAllTaskTypes()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskTypeDto>>>(HttpMethod.Get, "/taskTypes",
            ServiceType.ExpressService);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TaskTypeDto>>> GetTaskType(int id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskTypeDto>>(HttpMethod.Get, $"/taskTypes/{id}",
            ServiceType.ExpressService);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<TaskTypeDto>>> CreateTaskType(TaskTypeDto dto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskTypeDto>>(HttpMethod.Post, "/taskTypes",
            ServiceType.ExpressService, body: dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<TaskTypeDto>>> UpdateTaskType(int id, TaskTypeDto dto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TaskTypeDto>>(HttpMethod.Put, $"/taskTypes/{id}",
            ServiceType.ExpressService, body: dto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTaskType(int id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete, $"/taskTypes/{id}",
            ServiceType.ExpressService);
    }
}