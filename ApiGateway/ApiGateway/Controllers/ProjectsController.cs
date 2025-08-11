using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ProjectsController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetAllProjects()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(HttpMethod.Get, "/projects",
            ServiceType.SpringService);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<ProjectDto>>(HttpMethod.Get, $"/projects/{id}",
            ServiceType.SpringService);
    }

    [HttpGet("active-count")]
    public async Task<ActionResult<ApiResponse<long>>> GetActiveProjectCount()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<long>>(HttpMethod.Get, "/projects/active-count",
            ServiceType.SpringService);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateProject(CreateProjectDto projectDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<Guid>>(HttpMethod.Post, "/projects",
            ServiceType.SpringService, body: projectDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, CreateProjectDto projectDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<ProjectDto>>(HttpMethod.Put, $"/projects/{id}",
            ServiceType.SpringService, body: projectDto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteProject(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete, $"/projects/{id}",
            ServiceType.SpringService);
    }
}