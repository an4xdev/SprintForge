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
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>?>>(HttpMethod.Get,
            $"/tasks/unassigned/sprint/{id}", ServiceType.LaravelService);
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
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(HttpMethod.Get,
            $"/tasks/developer/{id}", ServiceType.LaravelService);
    }

    [Authorize(Roles = "admin,developer")]
    [HttpGet("developer/{id:guid}-ext")]
    public async Task<ActionResult<ApiResponse<List<TaskExtDto>>>> GetTasksByDeveloperExt(Guid id)
    {
        try
        {
            var tasksResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(
                HttpMethod.Get, $"/tasks/developer/{id}", ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(tasksResponse))
            {
                return Result<List<TaskExtDto>>.InternalError("Failed to retrieve developer tasks").ToActionResult();
            }

            var tasksData = ApiResponseExtensions.GetResultDataReference(tasksResponse);
            
            if (tasksData == null || tasksData.Count == 0)
            {
                return Result<List<TaskExtDto>>.Success(new List<TaskExtDto>(), "No tasks found for this developer")
                    .ToActionResult();
            }

            var taskTypesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskTypeDto>>>(
                HttpMethod.Get, "/taskTypes", ServiceType.ExpressService);

            var taskStatusesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskStatusDto>>>(
                HttpMethod.Get, "/taskStatuses", ServiceType.LaravelService);

            var sprintsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<SprintDto>>>(
                HttpMethod.Get, "/sprints", ServiceType.FastApiService);

            var projectsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(
                HttpMethod.Get, "/projects", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(taskTypesResponse) || !Utils.IsSuccessResult(taskStatusesResponse) ||
                !Utils.IsSuccessResult(sprintsResponse) || !Utils.IsSuccessResult(projectsResponse))
            {
                return Result<List<TaskExtDto>>.InternalError("Failed to retrieve related data").ToActionResult();
            }

            var taskTypesData = ApiResponseExtensions.GetResultDataReference(taskTypesResponse);
            var taskStatusesData = ApiResponseExtensions.GetResultDataReference(taskStatusesResponse);
            var sprintsData = ApiResponseExtensions.GetResultDataReference(sprintsResponse);
            var projectsData = ApiResponseExtensions.GetResultDataReference(projectsResponse);

            var taskTypesDict = taskTypesData?.ToDictionary(t => t.Id, t => t) ?? new Dictionary<int, TaskTypeDto>();
            var taskStatusesDict = taskStatusesData?.ToDictionary(s => s.Id, s => s) ??
                                   new Dictionary<int, TaskStatusDto>();
            var sprintsDict = sprintsData?.ToDictionary(s => s.Id, s => s) ?? new Dictionary<Guid, SprintDto>();
            var projectsDict = projectsData?.ToDictionary(p => p.Id, p => p) ?? new Dictionary<Guid, ProjectDto>();

            var userResponse = await sendRequestService.SendRequestAsync<ApiResponse<UserResponse>>(
                HttpMethod.Get, $"/users/{id}", ServiceType.AuthService);

            DeveloperBasicDto? developer = null;
            if (Utils.IsSuccessResult(userResponse))
            {
                var userData = ApiResponseExtensions.GetResultDataReference(userResponse);
                if (userData != null)
                {
                    developer = new DeveloperBasicDto
                    {
                        Id = userData.Id,
                        Username = userData.Username,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName
                    };
                }
            }

            var tasksExt = tasksData.Select(task =>
            {
                if (!taskTypesDict.TryGetValue(task.TaskTypeId, out var taskType))
                {
                    taskType = new TaskTypeDto { Id = task.TaskTypeId, Name = "Unknown Type" };
                }

                if (!taskStatusesDict.TryGetValue(task.TaskStatusId, out var taskStatus))
                {
                    taskStatus = new TaskStatusDto { Id = task.TaskStatusId, Name = "Unknown Status" };
                }

                SprintBasicDto? sprint = null;
                if (task.SprintId.HasValue && sprintsDict.TryGetValue(task.SprintId.Value, out var spr))
                {
                    sprint = new SprintBasicDto
                    {
                        Id = spr.Id,
                        Name = spr.Name,
                        StartDate = spr.StartDate,
                        EndDate = spr.EndDate
                    };
                }

                ProjectBasicDto? project = null;
                if (sprint != null && sprintsData != null)
                {
                    var sprintWithProject = sprintsData.FirstOrDefault(s => s.Id == sprint.Id && s.ProjectId.HasValue);
                    if (sprintWithProject?.ProjectId.HasValue == true &&
                        projectsDict.TryGetValue(sprintWithProject.ProjectId.Value, out var proj))
                    {
                        project = new ProjectBasicDto
                        {
                            Id = proj.Id,
                            Name = proj.Name,
                            StartDate = proj.StartDate,
                            EndDate = proj.EndDate
                        };
                    }
                }

                return new TaskExtDto
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    TaskType = taskType,
                    TaskStatus = taskStatus,
                    Developer = developer,
                    Sprint = sprint,
                    Project = project
                };
            }).ToList();

            return Result<List<TaskExtDto>>.Success(tasksExt, "Developer tasks retrieved with extended data")
                .ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<TaskExtDto>>.InternalError(
                    "An unexpected error occurred while retrieving developer tasks")
                .ToActionResult();
        }
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

    [Authorize(Roles = "admin,manager")]
    [HttpGet("manager/{id:guid}")]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetTasksByManager(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(HttpMethod.Get,
            $"/tasks/manager/{id}", ServiceType.LaravelService);
    }

    [HttpGet("all-ext")]
    public async Task<ActionResult<ApiResponse<List<TaskExtDto>>>> GetAllTasksExt()
    {
        try
        {
            var tasksResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(
                HttpMethod.Get, "/tasks", ServiceType.LaravelService);

            var usersResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<UserResponse>>>(
                HttpMethod.Get, "/users", ServiceType.AuthService);

            var taskTypesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskTypeDto>>>(
                HttpMethod.Get, "/taskTypes", ServiceType.ExpressService);

            var taskStatusesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskStatusDto>>>(
                HttpMethod.Get, "/taskStatuses", ServiceType.LaravelService);

            var sprintsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<SprintDto>>>(
                HttpMethod.Get, "/sprints", ServiceType.FastApiService);

            var projectsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(
                HttpMethod.Get, "/projects", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(tasksResponse) || !Utils.IsSuccessResult(usersResponse) ||
                !Utils.IsSuccessResult(taskTypesResponse) || !Utils.IsSuccessResult(taskStatusesResponse) ||
                !Utils.IsSuccessResult(sprintsResponse) || !Utils.IsSuccessResult(projectsResponse))
            {
                return Result<List<TaskExtDto>>.InternalError("Failed to retrieve tasks or related data")
                    .ToActionResult();
            }

            var tasksData = ApiResponseExtensions.GetResultDataReference(tasksResponse);
            var usersData = ApiResponseExtensions.GetResultDataReference(usersResponse);
            var taskTypesData = ApiResponseExtensions.GetResultDataReference(taskTypesResponse);
            var taskStatusesData = ApiResponseExtensions.GetResultDataReference(taskStatusesResponse);
            var sprintsData = ApiResponseExtensions.GetResultDataReference(sprintsResponse);
            var projectsData = ApiResponseExtensions.GetResultDataReference(projectsResponse);

            if (tasksData == null)
            {
                return Result<List<TaskExtDto>>.NotFound("No tasks found").ToActionResult();
            }

            if (tasksData.Count == 0)
            {
                return Result<List<TaskExtDto>>.Success([], "No tasks available").ToActionResult();
            }


            var usersDict = usersData?.ToDictionary(u => u.Id, u => u) ?? new Dictionary<Guid, UserResponse>();
            var taskTypesDict = taskTypesData?.ToDictionary(t => t.Id, t => t) ?? new Dictionary<int, TaskTypeDto>();
            var taskStatusesDict = taskStatusesData?.ToDictionary(s => s.Id, s => s) ??
                                   new Dictionary<int, TaskStatusDto>();
            var sprintsDict = sprintsData?.ToDictionary(s => s.Id, s => s) ?? new Dictionary<Guid, SprintDto>();
            var projectsDict = projectsData?.ToDictionary(p => p.Id, p => p) ?? new Dictionary<Guid, ProjectDto>();


            var tasksExt = tasksData.Select(task =>
            {
                if (!taskTypesDict.TryGetValue(task.TaskTypeId, out var taskType))
                {
                    taskType = new TaskTypeDto { Id = task.TaskTypeId, Name = "Unknown Type" };
                }


                if (!taskStatusesDict.TryGetValue(task.TaskStatusId, out var taskStatus))
                {
                    taskStatus = new TaskStatusDto { Id = task.TaskStatusId, Name = "Unknown Status" };
                }


                DeveloperBasicDto? developer = null;
                if (task.DeveloperId.HasValue && usersDict.TryGetValue(task.DeveloperId.Value, out var dev))
                {
                    developer = new DeveloperBasicDto
                    {
                        Id = dev.Id,
                        Username = dev.Username,
                        FirstName = dev.FirstName,
                        LastName = dev.LastName
                    };
                }


                SprintBasicDto? sprint = null;
                if (task.SprintId.HasValue && sprintsDict.TryGetValue(task.SprintId.Value, out var spr))
                {
                    sprint = new SprintBasicDto
                    {
                        Id = spr.Id,
                        Name = spr.Name,
                        StartDate = spr.StartDate,
                        EndDate = spr.EndDate
                    };
                }

                ProjectBasicDto? project = null;
                if (sprint == null || sprintsData == null)
                {
                    return new TaskExtDto
                    {
                        Id = task.Id,
                        Name = task.Name,
                        Description = task.Description,
                        TaskType = taskType,
                        TaskStatus = taskStatus,
                        Developer = developer,
                        Sprint = sprint,
                        Project = project
                    };
                }

                var sprintWithProject = sprintsData.FirstOrDefault(s => s.Id == sprint.Id && s.ProjectId.HasValue);
                if (sprintWithProject?.ProjectId.HasValue == true &&
                    projectsDict.TryGetValue(sprintWithProject.ProjectId.Value, out var proj))
                {
                    project = new ProjectBasicDto
                    {
                        Id = proj.Id,
                        Name = proj.Name,
                        StartDate = proj.StartDate,
                        EndDate = proj.EndDate
                    };
                }

                return new TaskExtDto
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    TaskType = taskType,
                    TaskStatus = taskStatus,
                    Developer = developer,
                    Sprint = sprint,
                    Project = project
                };
            }).ToList();

            return Result<List<TaskExtDto>>.Success(tasksExt, "Tasks retrieved with extended data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<TaskExtDto>>.InternalError("An unexpected error occurred while retrieving tasks")
                .ToActionResult();
        }
    }

    [HttpGet("{id:guid}-ext")]
    public async Task<ActionResult<ApiResponse<TaskExtDto>>> GetTaskExt(Guid id)
    {
        try
        {
            var taskResponse = await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(
                HttpMethod.Get, $"/tasks/{id}", ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(taskResponse))
            {
                return Result<TaskExtDto>.InternalError("Failed to retrieve task data").ToActionResult();
            }

            var taskData = ApiResponseExtensions.GetResultDataReference(taskResponse);
            if (taskData == null)
            {
                return Result<TaskExtDto>.NotFound("Task not found").ToActionResult();
            }

            var taskTypeResponse = await sendRequestService.SendRequestAsync<ApiResponse<TaskTypeDto>>(
                HttpMethod.Get, $"/taskTypes/{taskData.TaskTypeId}", ServiceType.ExpressService);


            var taskStatusResponse = await sendRequestService.SendRequestAsync<ApiResponse<TaskStatusDto>>(
                HttpMethod.Get, $"/taskStatuses/{taskData.TaskStatusId}", ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(taskTypeResponse) || !Utils.IsSuccessResult(taskStatusResponse))
            {
                return Result<TaskExtDto>.InternalError("Failed to retrieve task type or status data").ToActionResult();
            }

            var taskTypeData = ApiResponseExtensions.GetResultDataReference(taskTypeResponse);
            var taskStatusData = ApiResponseExtensions.GetResultDataReference(taskStatusResponse);

            if (taskTypeData == null || taskStatusData == null)
            {
                return Result<TaskExtDto>.InternalError("Missing required task type or status data").ToActionResult();
            }


            DeveloperBasicDto? developer = null;
            if (taskData.DeveloperId.HasValue)
            {
                var developerResponse = await sendRequestService.SendRequestAsync<ApiResponse<UserResponse>>(
                    HttpMethod.Get, $"/users/{taskData.DeveloperId.Value}", ServiceType.AuthService);

                if (Utils.IsSuccessResult(developerResponse))
                {
                    var developerData = ApiResponseExtensions.GetResultDataReference(developerResponse);
                    if (developerData != null)
                    {
                        developer = new DeveloperBasicDto
                        {
                            Id = developerData.Id,
                            Username = developerData.Username,
                            FirstName = developerData.FirstName,
                            LastName = developerData.LastName
                        };
                    }
                }
            }


            SprintBasicDto? sprint = null;
            ProjectBasicDto? project = null;

            if (taskData.SprintId.HasValue)
            {
                var sprintResponse = await sendRequestService.SendRequestAsync<ApiResponse<SprintDto>>(
                    HttpMethod.Get, $"/sprints/{taskData.SprintId.Value}", ServiceType.FastApiService);

                if (Utils.IsSuccessResult(sprintResponse))
                {
                    var sprintData = ApiResponseExtensions.GetResultDataReference(sprintResponse);
                    if (sprintData != null)
                    {
                        sprint = new SprintBasicDto
                        {
                            Id = sprintData.Id,
                            Name = sprintData.Name,
                            StartDate = sprintData.StartDate,
                            EndDate = sprintData.EndDate
                        };


                        if (sprintData.ProjectId.HasValue)
                        {
                            var projectResponse = await sendRequestService.SendRequestAsync<ApiResponse<ProjectDto>>(
                                HttpMethod.Get, $"/projects/{sprintData.ProjectId.Value}", ServiceType.SpringService);

                            if (Utils.IsSuccessResult(projectResponse))
                            {
                                var projectData = ApiResponseExtensions.GetResultDataReference(projectResponse);
                                if (projectData != null)
                                {
                                    project = new ProjectBasicDto
                                    {
                                        Id = projectData.Id,
                                        Name = projectData.Name,
                                        StartDate = projectData.StartDate,
                                        EndDate = projectData.EndDate
                                    };
                                }
                            }
                        }
                    }
                }
            }


            var taskExt = new TaskExtDto
            {
                Id = taskData.Id,
                Name = taskData.Name,
                Description = taskData.Description,
                TaskType = taskTypeData,
                TaskStatus = taskStatusData,
                Developer = developer,
                Sprint = sprint,
                Project = project
            };

            return Result<TaskExtDto>.Success(taskExt, "Task retrieved with extended data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<TaskExtDto>.InternalError("An unexpected error occurred while retrieving task")
                .ToActionResult();
        }
    }
}