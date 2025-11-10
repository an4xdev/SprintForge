using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.DTOs.Responses;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TeamsController(ISendRequestService sendRequestService) : ControllerBase
{
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

    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<long>>> GetTeamsCount()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<long>>(HttpMethod.Get, "/teams/count",
            ServiceType.SpringService);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTeam(CreateTeamDto createTeamDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<Guid>>(HttpMethod.Post, "/teams",
            ServiceType.SpringService, body: createTeamDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<TeamDto>>> UpdateTeam(Guid id, CreateTeamDto createTeamDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TeamDto>>(HttpMethod.Put, $"/teams/{id}",
            ServiceType.SpringService, body: createTeamDto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTeam(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete, $"/teams/{id}",
            ServiceType.SpringService);
    }

    [HttpGet("manager/{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<TeamDto>>> GetTeamByManager(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<TeamDto>>(HttpMethod.Get,
            $"/teams/manager/{id}", ServiceType.SpringService);
    }

    [HttpGet("all-ext")]
    public async Task<ActionResult<ApiResponse<List<TeamExtDto>>>> GetAllTeamsExt()
    {
        try
        {
            var teamsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TeamDto>>>(
                HttpMethod.Get, "/teams", ServiceType.SpringService);

            var usersResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<UserResponse>>>(
                HttpMethod.Get, "/users", ServiceType.AuthService);

            var projectsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(
                HttpMethod.Get, "/projects", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(teamsResponse) || !Utils.IsSuccessResult(usersResponse) || 
                !Utils.IsSuccessResult(projectsResponse))
            {
                return Result<List<TeamExtDto>>.InternalError("Failed to retrieve teams, users or projects data").ToActionResult();
            }

            var teamsData = ApiResponseExtensions.GetResultDataReference(teamsResponse);
            var usersData = ApiResponseExtensions.GetResultDataReference(usersResponse);
            var projectsData = ApiResponseExtensions.GetResultDataReference(projectsResponse);

            if (teamsData == null)
            {
                return Result<List<TeamExtDto>>.NotFound("No teams found").ToActionResult();
            }

            if (teamsData.Count == 0)
            {
                return Result<List<TeamExtDto>>.Success([], "No teams available").ToActionResult();
            }

            var usersDict = usersData?.ToDictionary(u => u.Id, u => u) ?? new Dictionary<Guid, UserResponse>();
            var projectsDict = projectsData?.ToDictionary(p => p.Id, p => p) ?? new Dictionary<Guid, ProjectDto>();

            var teamsExt = new List<TeamExtDto>();

            foreach (var team in teamsData)
            {
                var teamId = Guid.Parse(team.Id);
                var managerId = Guid.Parse(team.Manager.Id);

                if (!usersDict.TryGetValue(managerId, out var manager))
                    continue;

                var teamMembersResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<UserResponse>>>(
                    HttpMethod.Get, $"/users/teams/{teamId}", ServiceType.AuthService);

                var teamMembers = Utils.IsSuccessResult(teamMembersResponse) 
                    ? ApiResponseExtensions.GetResultDataReference(teamMembersResponse) ?? []
                    : [];

                var developers = teamMembers
                    .Where(member => member.Id != managerId)
                    .Select(dev => new DeveloperBasicDto
                    {
                        Id = dev.Id,
                        Username = dev.Username,
                        FirstName = dev.FirstName,
                        LastName = dev.LastName
                    })
                    .ToList();

                var teamProject = projectsDict.Values
                    .FirstOrDefault() ?? new ProjectDto
                {
                    Id = Guid.Empty,
                    Name = "No project assigned",
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                    CompanyDto = new CompanyDto { Id = 0, Name = "Unknown" }
                };

                var teamExt = new TeamExtDto
                {
                    Id = teamId,
                    Name = team.Name,
                    Manager = new ManagerExtendedDto
                    {
                        Id = manager.Id,
                        Username = manager.Username,
                        FirstName = manager.FirstName,
                        LastName = manager.LastName,
                        Email = manager.Email
                    },
                    Project = new ProjectBasicDto
                    {
                        Id = teamProject.Id,
                        Name = teamProject.Name,
                        StartDate = teamProject.StartDate,
                        EndDate = teamProject.EndDate
                    },
                    Developers = developers
                };

                teamsExt.Add(teamExt);
            }

            return Result<List<TeamExtDto>>.Success(teamsExt, "Teams retrieved with extended data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<TeamExtDto>>.InternalError("An unexpected error occurred while retrieving teams").ToActionResult();
        }
    }

    [HttpGet("{id:guid}-ext")]
    public async Task<ActionResult<ApiResponse<TeamExtDto>>> GetTeamExt(Guid id)
    {
        try
        {
            var teamResponse = await sendRequestService.SendRequestAsync<ApiResponse<TeamDto>>(
                HttpMethod.Get, $"/teams/{id}", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(teamResponse))
            {
                return Result<TeamExtDto>.InternalError("Failed to retrieve team data").ToActionResult();
            }

            var teamData = ApiResponseExtensions.GetResultDataReference(teamResponse);
            if (teamData == null)
            {
                return Result<TeamExtDto>.NotFound("Team not found").ToActionResult();
            }

            var team = teamData;
            var managerId = Guid.Parse(team.Manager.Id);

            var managerResponse = await sendRequestService.SendRequestAsync<ApiResponse<UserResponse>>(
                HttpMethod.Get, $"/users/{managerId}", ServiceType.AuthService);

            var teamMembersResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<UserResponse>>>(
                HttpMethod.Get, $"/users/teams/{id}", ServiceType.AuthService);

            if (!Utils.IsSuccessResult(managerResponse) || !Utils.IsSuccessResult(teamMembersResponse))
            {
                return Result<TeamExtDto>.InternalError("Failed to retrieve manager or team members data").ToActionResult();
            }

            var managerData = ApiResponseExtensions.GetResultDataReference(managerResponse);
            var teamMembersData = ApiResponseExtensions.GetResultDataReference(teamMembersResponse) ?? [];

            if (managerData == null)
            {
                return Result<TeamExtDto>.InternalError("Manager data not found").ToActionResult();
            }

            var developers = teamMembersData
                .Where(member => member.Id != managerId)
                .Select(dev => new DeveloperBasicDto
                {
                    Id = dev.Id,
                    Username = dev.Username,
                    FirstName = dev.FirstName,
                    LastName = dev.LastName
                })
                .ToList();

            var projectsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(
                HttpMethod.Get, "/projects", ServiceType.SpringService);

            var project = new ProjectBasicDto
            {
                Id = Guid.Empty,
                Name = "No project assigned",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
            };

            if (Utils.IsSuccessResult(projectsResponse))
            {
                var projectsData = ApiResponseExtensions.GetResultDataReference(projectsResponse);
                var teamProject = projectsData?.FirstOrDefault();

                if (teamProject != null)
                {
                    project = new ProjectBasicDto
                    {
                        Id = teamProject.Id,
                        Name = teamProject.Name,
                        StartDate = teamProject.StartDate,
                        EndDate = teamProject.EndDate
                    };
                }
            }

            var teamExt = new TeamExtDto
            {
                Id = Guid.Parse(team.Id),
                Name = team.Name,
                Manager = new ManagerExtendedDto
                {
                    Id = managerData.Id,
                    Username = managerData.Username,
                    FirstName = managerData.FirstName,
                    LastName = managerData.LastName,
                    Email = managerData.Email
                },
                Project = project,
                Developers = developers
            };

            return Result<TeamExtDto>.Success(teamExt, "Team retrieved with extended data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<TeamExtDto>.InternalError("An unexpected error occurred while retrieving team").ToActionResult();
        }
    }
}