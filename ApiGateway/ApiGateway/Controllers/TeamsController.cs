using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TeamsController(ISendRequestService sendRequestService) : ControllerBase
{
    // TODO: implement
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TeamDto>>>> GetAllTeams()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TeamDto>>>(HttpMethod.Get, "/teams",
            ServiceType.SpringService);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TeamDto>>> GetTeam(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TeamDto>>(HttpMethod.Get, $"/teams/{id}",
            ServiceType.SpringService);
    }

    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTeam(CreateTeamDto createTeamDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<Guid>>(HttpMethod.Post, "/teams",
            ServiceType.SpringService, body: createTeamDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<TeamDto>>> UpdateTeam(Guid id, CreateTeamDto createTeamDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TeamDto>>(HttpMethod.Put, $"/teams/{id}",
            ServiceType.SpringService, body: createTeamDto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTeam(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete, $"/teams/{id}",
            ServiceType.SpringService);
    }
}