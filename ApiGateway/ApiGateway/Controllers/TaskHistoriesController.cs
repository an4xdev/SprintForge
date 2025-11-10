using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class TaskHistoriesController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<TaskHistoryDto>>>> GetAllTaskHistories()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskHistoryDto>>>(
            HttpMethod.Get,
            "/taskHistories",
            ServiceType.TaskHistoriesService);
    }

    [HttpGet("ext")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<TaskHistoryExtDto>>>> GetAllTaskHistoriesExt()
    {
        try
        {
            var historiesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskHistoryDto>>>(
                HttpMethod.Get,
                "/taskHistories",
                ServiceType.TaskHistoriesService);

            if (!Utils.IsSuccessResult(historiesResponse))
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Failed to retrieve task histories").ToActionResult();
            }

            var historiesData = ApiResponseExtensions.GetResultDataReference(historiesResponse);
            if (historiesData == null || historiesData.Count == 0)
            {
                return Result<List<TaskHistoryExtDto>>.Success([], "No task histories found").ToActionResult();
            }

            var tasksResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(
                HttpMethod.Get,
                "/tasks",
                ServiceType.LaravelService);

            var statusesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskStatusDto>>>(
                HttpMethod.Get,
                "/taskStatuses",
                ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(tasksResponse) || !Utils.IsSuccessResult(statusesResponse))
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Failed to retrieve tasks or status data").ToActionResult();
            }

            var tasksData = ApiResponseExtensions.GetResultDataReference(tasksResponse);
            var statusesData = ApiResponseExtensions.GetResultDataReference(statusesResponse);

            if (tasksData == null || statusesData == null)
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Missing required tasks or status data").ToActionResult();
            }

            var tasksDict = tasksData.ToDictionary(t => t.Id, t => t);
            var statusesDict = statusesData
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.First());

            var historiesExt = historiesData
                .Where(history => tasksDict.ContainsKey(history.TaskId))
                .Select(history =>
                {
                    var task = tasksDict[history.TaskId];
                    var oldStatusDto = !string.IsNullOrEmpty(history.OldStatus) && statusesDict.TryGetValue(history.OldStatus, out var oldSt) ? oldSt : null;
                    var newStatusDto = statusesDict.TryGetValue(history.NewStatus, out var newSt) ? newSt : null;

                    if (newStatusDto == null)
                        return null;

                    return new TaskHistoryExtDto
                    {
                        Id = history.Id,
                        Task = new TaskBasicDto
                        {
                            Id = task.Id,
                            Name = task.Name,
                            Description = task.Description
                        },
                        ChangeDate = history.ChangeDate,
                        OldStatus = oldStatusDto,
                        NewStatus = newStatusDto,
                    };
                })
                .Where(h => h != null)
                .Cast<TaskHistoryExtDto>()
                .ToList();

            return Result<List<TaskHistoryExtDto>>.Success(historiesExt, "Task histories retrieved with extended data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<TaskHistoryExtDto>>.InternalError("An unexpected error occurred while retrieving task histories").ToActionResult();
        }
    }

    [HttpGet("task/{taskId:guid}")]
    [Authorize(Roles = "admin,manager,developer")]
    public async Task<ActionResult<ApiResponse<List<TaskHistoryDto>>>> GetTaskHistoriesByTask(Guid taskId)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<TaskHistoryDto>>>(
            HttpMethod.Get,
            $"/taskHistories?taskId={taskId}",
            ServiceType.TaskHistoriesService);
    }

    [HttpGet("task/{taskId:guid}/ext")]
    [Authorize(Roles = "admin,manager,developer")]
    public async Task<ActionResult<ApiResponse<List<TaskHistoryExtDto>>>> GetTaskHistoriesByTaskExt(Guid taskId)
    {
        try
        {
            var historiesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskHistoryDto>>>(
                HttpMethod.Get,
                $"/taskHistories?taskId={taskId}",
                ServiceType.TaskHistoriesService);

            if (!Utils.IsSuccessResult(historiesResponse))
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Failed to retrieve task histories").ToActionResult();
            }

            var historiesData = ApiResponseExtensions.GetResultDataReference(historiesResponse);
            if (historiesData == null || historiesData.Count == 0)
            {
                return Result<List<TaskHistoryExtDto>>.Success([], "No task histories found").ToActionResult();
            }

            var taskResponse = await sendRequestService.SendRequestAsync<ApiResponse<TaskDto>>(
                HttpMethod.Get,
                $"/tasks/{taskId}",
                ServiceType.LaravelService);

            var statusesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskStatusDto>>>(
                HttpMethod.Get,
                "/taskStatuses",
                ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(taskResponse) || !Utils.IsSuccessResult(statusesResponse))
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Failed to retrieve task or status data").ToActionResult();
            }

            var taskData = ApiResponseExtensions.GetResultDataReference(taskResponse);
            var statusesData = ApiResponseExtensions.GetResultDataReference(statusesResponse);

            if (taskData == null || statusesData == null)
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Missing required task or status data").ToActionResult();
            }

            var statusesDict = statusesData
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.First());

            var historiesExt = historiesData.Select(history =>
            {
                var oldStatusDto = !string.IsNullOrEmpty(history.OldStatus) && statusesDict.TryGetValue(history.OldStatus, out var oldSt) ? oldSt : null;
                var newStatusDto = statusesDict.TryGetValue(history.NewStatus, out var newSt) ? newSt : null;

                return new TaskHistoryExtDto
                {
                    Id = history.Id,
                    Task = new TaskBasicDto
                    {
                        Id = taskData.Id,
                        Name = taskData.Name,
                        Description = taskData.Description
                    },
                    ChangeDate = history.ChangeDate,
                    OldStatus = oldStatusDto,
                    NewStatus = newStatusDto!,
                };
            }).ToList();

            return Result<List<TaskHistoryExtDto>>.Success(historiesExt, "Task histories retrieved with extended data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<TaskHistoryExtDto>>.InternalError("An unexpected error occurred while retrieving task histories").ToActionResult();
        }
    }

    [HttpGet("manager/{managerId:guid}/ext")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<List<TaskHistoryExtDto>>>> GetTaskHistoriesByManagerExt(Guid managerId)
    {
        try
        {
            var managerTasksResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskDto>>>(
                HttpMethod.Get,
                $"/tasks/manager/{managerId}",
                ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(managerTasksResponse))
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Failed to retrieve manager's tasks").ToActionResult();
            }

            var managerTasksData = ApiResponseExtensions.GetResultDataReference(managerTasksResponse);
            if (managerTasksData == null || managerTasksData.Count == 0)
            {
                return Result<List<TaskHistoryExtDto>>.Success([], "No tasks found for this manager").ToActionResult();
            }

            var taskIds = managerTasksData.Select(t => t.Id).ToList();

            var historiesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskHistoryDto>>>(
                HttpMethod.Get,
                "/taskHistories",
                ServiceType.TaskHistoriesService);

            if (!Utils.IsSuccessResult(historiesResponse))
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Failed to retrieve task histories").ToActionResult();
            }

            var historiesData = ApiResponseExtensions.GetResultDataReference(historiesResponse);
            if (historiesData == null || historiesData.Count == 0)
            {
                return Result<List<TaskHistoryExtDto>>.Success([], "No task histories found").ToActionResult();
            }

            var filteredHistories = historiesData.Where(h => taskIds.Contains(h.TaskId)).ToList();

            if (filteredHistories.Count == 0)
            {
                return Result<List<TaskHistoryExtDto>>.Success([], "No task histories found for manager's tasks").ToActionResult();
            }

            var statusesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<TaskStatusDto>>>(
                HttpMethod.Get,
                "/taskStatuses",
                ServiceType.LaravelService);

            if (!Utils.IsSuccessResult(statusesResponse))
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Failed to retrieve task statuses").ToActionResult();
            }

            var statusesData = ApiResponseExtensions.GetResultDataReference(statusesResponse);
            if (statusesData == null)
            {
                return Result<List<TaskHistoryExtDto>>.InternalError("Missing task statuses data").ToActionResult();
            }

            var tasksDict = managerTasksData.ToDictionary(t => t.Id, t => t);
            var statusesDict = statusesData
                .GroupBy(s => s.Name)
                .ToDictionary(g => g.Key, g => g.First());

            var historiesExt = filteredHistories
                .Where(history => tasksDict.ContainsKey(history.TaskId))
                .Select(history =>
                {
                    var task = tasksDict[history.TaskId];
                    var oldStatusDto = !string.IsNullOrEmpty(history.OldStatus) && statusesDict.TryGetValue(history.OldStatus, out var oldSt) ? oldSt : null;
                    var newStatusDto = statusesDict.TryGetValue(history.NewStatus, out var newSt) ? newSt : null;

                    if (newStatusDto == null)
                        return null;

                    return new TaskHistoryExtDto
                    {
                        Id = history.Id,
                        Task = new TaskBasicDto
                        {
                            Id = task.Id,
                            Name = task.Name,
                            Description = task.Description
                        },
                        ChangeDate = history.ChangeDate,
                        OldStatus = oldStatusDto,
                        NewStatus = newStatusDto,
                    };
                })
                .Where(h => h != null)
                .Cast<TaskHistoryExtDto>()
                .ToList();

            return Result<List<TaskHistoryExtDto>>.Success(historiesExt, "Task histories for manager's team retrieved successfully").ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<TaskHistoryExtDto>>.InternalError("An unexpected error occurred while retrieving task histories").ToActionResult();
        }
    }
}