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
        return await sendRequestService.SendRequestAsync<ApiResponse<Guid>>(HttpMethod.Get,
            $"/sprints/manager/{id}/last", ServiceType.FastApiService);
    }

    [HttpGet("manager/{id:guid}-ext")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<SprintExtDto>>>> GetSprintsByManagerExt(Guid id)
    {
        try
        {
            var sprintsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<SprintDto>>>(
                HttpMethod.Get, $"/sprints/manager/{id}", ServiceType.FastApiService);

            if (!Utils.IsSuccessResult(sprintsResponse))
            {
                return Result<List<SprintExtDto>>.InternalError("Failed to retrieve sprints data").ToActionResult();
            }

            var sprintsData = ApiResponseExtensions.GetResultDataReference(sprintsResponse);
            if (sprintsData == null || sprintsData.Count == 0)
            {
                return Result<List<SprintExtDto>>.Success([], "No sprints found for manager")
                    .ToActionResult();
            }


            var managerResponse = await sendRequestService.SendRequestAsync<ApiResponse<UserResponse>>(
                HttpMethod.Get, $"/users/{id}", ServiceType.AuthService);


            var teamsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TeamDto>>>(
                HttpMethod.Get, "/teams", ServiceType.SpringService);


            var projectsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(
                HttpMethod.Get, "/projects", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(managerResponse) || !Utils.IsSuccessResult(teamsResponse) ||
                !Utils.IsSuccessResult(projectsResponse))
            {
                return Result<List<SprintExtDto>>.InternalError("Failed to retrieve manager, teams or projects data")
                    .ToActionResult();
            }

            var managerData = ApiResponseExtensions.GetResultDataReference(managerResponse);
            var teamsData = ApiResponseExtensions.GetResultDataReference(teamsResponse);
            var projectsData = ApiResponseExtensions.GetResultDataReference(projectsResponse);

            if (managerData == null)
            {
                return Result<List<SprintExtDto>>.NotFound("Manager not found").ToActionResult();
            }


            var teamsDict = teamsData?.ToDictionary(t => Guid.Parse(t.Id), t => t) ?? new Dictionary<Guid, TeamDto>();
            var projectsDict = projectsData?.ToDictionary(p => p.Id, p => p) ?? new Dictionary<Guid, ProjectDto>();


            var sprintsExt = sprintsData
                .Where(sprint => sprint.ProjectId.HasValue)
                .Select(sprint =>
                {
                    var team = teamsDict.TryGetValue(sprint.TeamId, out var t) ? t : null;
                    var project = projectsDict.TryGetValue(sprint.ProjectId!.Value, out var p) ? p : null;


                    if (team == null || project == null)
                        return null;

                    return new SprintExtDto
                    {
                        Id = sprint.Id,
                        Name = sprint.Name,
                        StartDate = sprint.StartDate,
                        EndDate = sprint.EndDate,
                        Manager = new ManagerBasicDto
                        {
                            Id = managerData.Id,
                            Username = managerData.Username,
                            FirstName = managerData.FirstName,
                            LastName = managerData.LastName
                        },
                        Team = new TeamBasicDto
                        {
                            Id = Guid.Parse(team.Id),
                            Name = team.Name
                        },
                        Project = new ProjectBasicDto
                        {
                            Id = project.Id,
                            Name = project.Name,
                            StartDate = project.StartDate,
                            EndDate = project.EndDate
                        }
                    };
                })
                .Where(s => s != null)
                .Cast<SprintExtDto>()
                .ToList();

            return Result<List<SprintExtDto>>.Success(sprintsExt, "Manager sprints retrieved with extended data")
                .ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<SprintExtDto>>
                .InternalError("An unexpected error occurred while retrieving manager sprints").ToActionResult();
        }
    }

    [HttpGet("all-ext")]
    public async Task<ActionResult<ApiResponse<List<SprintExtDto>>>> GetAllSprintsExt()
    {
        try
        {
            var sprintsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<SprintDto>>>(
                HttpMethod.Get, "/sprints", ServiceType.FastApiService);


            var usersResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<UserResponse>>>(
                HttpMethod.Get, "/users", ServiceType.AuthService);


            var teamsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TeamDto>>>(
                HttpMethod.Get, "/teams", ServiceType.SpringService);


            var projectsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(
                HttpMethod.Get, "/projects", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(sprintsResponse) || !Utils.IsSuccessResult(usersResponse) ||
                !Utils.IsSuccessResult(teamsResponse) || !Utils.IsSuccessResult(projectsResponse))
            {
                return Result<List<SprintExtDto>>
                    .InternalError("Failed to retrieve sprints, users, teams or projects data").ToActionResult();
            }

            var sprintsData = ApiResponseExtensions.GetResultDataReference(sprintsResponse);
            var usersData = ApiResponseExtensions.GetResultDataReference(usersResponse);
            var teamsData = ApiResponseExtensions.GetResultDataReference(teamsResponse);
            var projectsData = ApiResponseExtensions.GetResultDataReference(projectsResponse);

            if (sprintsData == null)
            {
                return Result<List<SprintExtDto>>.NotFound("No sprints found").ToActionResult();
            }

            if (!sprintsData.Any())
            {
                return Result<List<SprintExtDto>>.Success(new List<SprintExtDto>(), "No sprints available")
                    .ToActionResult();
            }


            var usersDict = usersData?.ToDictionary(u => u.Id, u => u) ?? new Dictionary<Guid, UserResponse>();
            var teamsDict = teamsData?.ToDictionary(t => Guid.Parse(t.Id), t => t) ?? new Dictionary<Guid, TeamDto>();
            var projectsDict = projectsData?.ToDictionary(p => p.Id, p => p) ?? new Dictionary<Guid, ProjectDto>();


            var sprintsExt = sprintsData
                .Where(sprint => sprint.ProjectId.HasValue)
                .Select(sprint =>
                {
                    var manager = usersDict.TryGetValue(sprint.ManagerId, out var user) ? user : null;
                    var team = teamsDict.TryGetValue(sprint.TeamId, out var t) ? t : null;
                    var project = projectsDict.TryGetValue(sprint.ProjectId!.Value, out var p) ? p : null;


                    if (manager == null || team == null || project == null)
                        return null;

                    return new SprintExtDto
                    {
                        Id = sprint.Id,
                        Name = sprint.Name,
                        StartDate = sprint.StartDate,
                        EndDate = sprint.EndDate,
                        Manager = new ManagerBasicDto
                        {
                            Id = manager.Id,
                            Username = manager.Username,
                            FirstName = manager.FirstName,
                            LastName = manager.LastName
                        },
                        Team = new TeamBasicDto
                        {
                            Id = Guid.Parse(team.Id),
                            Name = team.Name
                        },
                        Project = new ProjectBasicDto
                        {
                            Id = project.Id,
                            Name = project.Name,
                            StartDate = project.StartDate,
                            EndDate = project.EndDate
                        }
                    };
                })
                .Where(s => s != null)
                .Cast<SprintExtDto>()
                .ToList();

            return Result<List<SprintExtDto>>.Success(sprintsExt, "Sprints retrieved with extended data")
                .ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<SprintExtDto>>.InternalError("An unexpected error occurred while retrieving sprints")
                .ToActionResult();
        }
    }

    [HttpGet("{id:guid}-ext")]
    public async Task<ActionResult<ApiResponse<SprintExtDto>>> GetSprintExt(Guid id)
    {
        try
        {
            var sprintResponse = await sendRequestService.SendRequestAsync<ApiResponse<SprintDto>>(
                HttpMethod.Get, $"/sprints/{id}", ServiceType.FastApiService);

            if (!Utils.IsSuccessResult(sprintResponse))
            {
                return Result<SprintExtDto>.InternalError("Failed to retrieve sprint data").ToActionResult();
            }

            var sprintData = ApiResponseExtensions.GetResultDataReference(sprintResponse);
            if (sprintData == null)
            {
                return Result<SprintExtDto>.NotFound("Sprint not found").ToActionResult();
            }

            if (!sprintData.ProjectId.HasValue)
            {
                return Result<SprintExtDto>.BadRequest("Sprint does not have associated project").ToActionResult();
            }

            var sprint = sprintData;


            var managerResponse = await sendRequestService.SendRequestAsync<ApiResponse<UserResponse>>(
                HttpMethod.Get, $"/users/{sprint.ManagerId}", ServiceType.AuthService);


            var teamResponse = await sendRequestService.SendRequestAsync<ApiResponse<TeamDto>>(
                HttpMethod.Get, $"/teams/{sprint.TeamId}", ServiceType.SpringService);


            var projectResponse = await sendRequestService.SendRequestAsync<ApiResponse<ProjectDto>>(
                HttpMethod.Get, $"/projects/{sprint.ProjectId.Value}", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(managerResponse) || !Utils.IsSuccessResult(teamResponse) ||
                !Utils.IsSuccessResult(projectResponse))
            {
                return Result<SprintExtDto>.InternalError("Failed to retrieve manager, team or project data")
                    .ToActionResult();
            }

            var managerData = ApiResponseExtensions.GetResultDataReference(managerResponse);
            var teamData = ApiResponseExtensions.GetResultDataReference(teamResponse);
            var projectData = ApiResponseExtensions.GetResultDataReference(projectResponse);

            if (managerData == null || teamData == null || projectData == null)
            {
                return Result<SprintExtDto>.InternalError("Missing required data for manager, team or project")
                    .ToActionResult();
            }


            var sprintExt = new SprintExtDto
            {
                Id = sprint.Id,
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Manager = new ManagerBasicDto
                {
                    Id = managerData.Id,
                    Username = managerData.Username,
                    FirstName = managerData.FirstName,
                    LastName = managerData.LastName
                },
                Team = new TeamBasicDto
                {
                    Id = Guid.Parse(teamData.Id),
                    Name = teamData.Name
                },
                Project = new ProjectBasicDto
                {
                    Id = projectData.Id,
                    Name = projectData.Name,
                    StartDate = projectData.StartDate,
                    EndDate = projectData.EndDate
                }
            };

            return Result<SprintExtDto>.Success(sprintExt, "Sprint retrieved with extended data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<SprintExtDto>.InternalError("An unexpected error occurred while retrieving sprint")
                .ToActionResult();
        }
    }
}