using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class SprintsController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SprintDto>>>> GetAllSprints()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<SprintDto>>>(HttpMethod.Get, "/sprints",
            ServiceType.FastApiService);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SprintDto>>> GetSprint(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<SprintDto>>(HttpMethod.Get, $"/sprints/{id}",
            ServiceType.FastApiService);
    }

    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<SprintDto>>> CreateSprint(CreateSprintDto sprintDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<SprintDto>>(HttpMethod.Post, "/sprints",
            ServiceType.FastApiService, body: sprintDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<SprintDto>>> UpdateSprint(Guid id, EditSprintDto sprintDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<SprintDto>>(HttpMethod.Put, $"/sprints/{id}",
            ServiceType.FastApiService, body: sprintDto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteSprint(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete, $"/sprints/{id}",
            ServiceType.FastApiService);
    }

    [HttpGet("manager/{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<SprintDto>>>> GetSprintsByManager(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<SprintDto>>>(HttpMethod.Get,
            $"/sprints/manager/{id}", ServiceType.FastApiService);
    }

    [HttpGet("manager/{id:guid}/last")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<Guid>>> GetSprintByManagerLast(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<Guid>>(HttpMethod.Get, $"/sprints/manager/{id}/last", ServiceType.FastApiService);
    }
}