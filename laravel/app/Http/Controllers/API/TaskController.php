<?php

namespace App\Http\Controllers\API;

use App\Http\Controllers\Controller;
use App\Http\Responses\ApiResponse;
use App\Models\Project;
use App\Models\Sprint;
use App\Models\Task;
use App\Models\TaskStatus;
use App\Models\TaskType;
use App\Models\User;
use App\Services\AuditService;
use Illuminate\Http\Request;
use Illuminate\Http\Response;
use Illuminate\Support\Str;
use Illuminate\Support\Facades\Log;
use Symfony\Component\HttpFoundation\Response as ResponseAlias;

class TaskController extends Controller
{
    private AuditService $auditService;

    public function __construct(AuditService $auditService)
    {
        $this->auditService = $auditService;
    }

    /**
     * Display a listing of the resource.
     */
    public function index()
    {
        Log::info('TaskController::index - Retrieving all tasks');
        $tasks = Task::all();
        Log::info('TaskController::index - Retrieved tasks', ['count' => $tasks->count()]);
        $response = ApiResponse::Success('Tasks retrieved successfully', $tasks);
        return response()->json($response);
    }

    /**
     * Store a newly created resource in storage.
     */
    public function store(Request $request)
    {
        Log::info('TaskController::store - Starting task creation', [
            'request_data' => $request->all()
        ]);

        try {
            $request->validate([
                'name' => 'required|string|max:255',
                'description' => 'nullable|string|max:255',
                'taskTypeId' => 'required|integer|exists:TaskTypes,Id',
            ]);

            Log::info('TaskController::store - Validation passed');

            if ($request->has('taskStatusId')) {
                Log::warning('TaskController::store - taskStatusId provided in request');
                $this->auditService->logAction('CREATE_FAILED', 'Task', 'Task status ID provided in request');
                $response = ApiResponse::BadRequest('Task status ID automatically assigned to `Created` status. Do not provide it in the request.');
                return response()->json($response, ResponseAlias::HTTP_BAD_REQUEST);
            }

            $statusId = TaskStatus::where('Name', 'Created')->value('Id');
            Log::info('TaskController::store - Found status ID', ['statusId' => $statusId]);

            if (!$statusId) {
                Log::error('TaskController::store - Default task status "Created" not found');
                $this->auditService->logAction('CREATE_FAILED', 'Task', 'Default task status "Created" not found');
                $response = ApiResponse::InternalError('Default task status "Created" not found');
                return response()->json($response, ResponseAlias::HTTP_INTERNAL_SERVER_ERROR);
            }

            if ($request->has('developerId') && $request->input('developerId') !== null) {
                Log::info('TaskController::store - Checking developer', ['developerId' => $request->input('developerId')]);
                if (User::where('Id', $request->input('developerId'))->doesntExist()) {
                    Log::warning('TaskController::store - Developer not found');
                    $this->auditService->logAction('CREATE_FAILED', 'Task', 'Developer not found');
                    $response = ApiResponse::BadRequest('Developer not found');
                    return response()->json($response, ResponseAlias::HTTP_BAD_REQUEST);
                }
                $assignedStatusId = TaskStatus::where('Name', 'Assigned')->value('Id');
                if ($assignedStatusId) {
                    $statusId = $assignedStatusId;
                    Log::info('TaskController::store - Developer assigned, changing status to Assigned');
                }
            }

            if ($request->has('sprintId') && $request->input('sprintId') !== null) {
                Log::info('TaskController::store - Checking sprint', ['sprintId' => $request->input('sprintId')]);
                if (Sprint::where('Id', $request->input('sprintId'))->doesntExist()) {
                    Log::warning('TaskController::store - Sprint not found');
                    $this->auditService->logAction('CREATE_FAILED', 'Task', 'Sprint not found');
                    $response = ApiResponse::BadRequest('Sprint not found');
                    return response()->json($response, ResponseAlias::HTTP_BAD_REQUEST);
                }
            }

            $taskData = [
                'Id' => Str::uuid()->toString(),
                'Name' => $request->input('name'),
                'Description' => $request->input('description', null),
                'TaskTypeId' => $request->input('taskTypeId'),
                'TaskStatusId' => $statusId,
                'DeveloperId' => $request->input('developerId', null),
                'SprintId' => null,
            ];

            Log::info('TaskController::store - Creating task', ['taskData' => $taskData]);

            $task = Task::create($taskData);

            Log::info('TaskController::store - Task created successfully', ['taskId' => $task->Id]);

            $this->auditService->logAction('CREATE_SUCCESS', 'Task', 'Created new task: ' . $request->input('name'));

            $response = ApiResponse::Created('Task created successfully', $task);
            return response()->json($response, Response::HTTP_CREATED);
        } catch (\Exception $e) {
            Log::error('TaskController::store - Exception occurred', [
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString()
            ]);
            throw $e;
        }
    }

    /**
     * Display the specified resource.
     */
    public function show(Task $task)
    {
        Log::info('TaskController::show - Retrieving task', ['taskId' => $task->Id ?? 'null']);
        if (!$task) {
            Log::warning('TaskController::show - Task not found');
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }
        Log::info('TaskController::show - Task retrieved successfully', ['taskId' => $task->Id]);
        $response = ApiResponse::Success('Task retrieved successfully', $task);
        return response()->json($response);
    }

    /**
     * Update the specified resource in storage.
     */
    public function update(Request $request, Task $task)
    {
        Log::info('TaskController::update - Starting task update', [
            'taskId' => $task->Id ?? 'null',
            'request_data' => $request->all()
        ]);

        if (!$task) {
            Log::warning('TaskController::update - Task not found');
            $this->auditService->logAction('UPDATE_FAILED', 'Task', 'Task not found');
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        try {
            $request->validate([
                'name' => 'nullable|string|max:255',
                'description' => 'nullable|string|max:255',
                'taskTypeId' => 'nullable|integer|exists:TaskTypes,Id',
            ]);

            Log::info('TaskController::update - Validation passed');

            if ($request->has('developerId')) {
                Log::warning('TaskController::update - Attempted to update developerId directly');
                $this->auditService->logAction('UPDATE_FAILED', 'Task', 'Cannot update developerId directly. Use assign task to developer method.');
                $response = ApiResponse::BadRequest('Cannot update developerId directly. Use assign task to developer method.');
                return response()->json($response, ResponseAlias::HTTP_BAD_REQUEST);
            }

            if ($request->has('sprintId')) {
                Log::warning('TaskController::update - Attempted to update sprintId directly');
                $this->auditService->logAction('UPDATE_FAILED', 'Task', 'Cannot update sprintId directly. Use move task to another sprint method.');
                $response = ApiResponse::BadRequest('Cannot update sprintId directly. Use move task to another sprint method.');
                return response()->json($response, ResponseAlias::HTTP_BAD_REQUEST);
            }

            $task->update(
                [
                    'Name' => $request->input('name', $task->Name),
                    'Description' => $request->input('description', $task->Description),
                    'TaskTypeId' => $request->input('taskTypeId', $task->TaskTypeId),
                ]
            );

            Log::info('TaskController::update - Task updated successfully', ['taskId' => $task->Id]);
            $this->auditService->logAction('UPDATE_SUCCESS', 'Task', 'Updated task: ' . $task->Name);
            $response = ApiResponse::Success('Task updated successfully', $task);
            return response()->json($response);
        } catch (\Exception $e) {
            Log::error('TaskController::update - Exception occurred', [
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString()
            ]);
            throw $e;
        }
    }

    /**
     * Assign a developer to a task
     */
    public function assignDeveloper(Request $request, Task $task)
    {
        Log::info('TaskController::assignDeveloper - Starting developer assignment', [
            'taskId' => $task->Id ?? 'null',
            'request_data' => $request->all()
        ]);

        if (!$task) {
            Log::warning('TaskController::assignDeveloper - Task not found');
            $this->auditService->logAction('UPDATE_FAILED', 'Task', 'Task not found');
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        try {
            $request->validate([
                'developerId' => 'required|uuid|exists:Users,Id',
            ]);

            Log::info('TaskController::assignDeveloper - Validation passed');

            $task->DeveloperId = $request->input('developerId');
            $task->save();

            Log::info('TaskController::assignDeveloper - Developer assigned successfully', [
                'taskId' => $task->Id,
                'developerId' => $request->input('developerId')
            ]);

            $this->auditService->logAction('UPDATE_SUCCESS', 'Task', 'Assigned developer to task: ' . $task->Name);
            $response = ApiResponse::Success('Developer assigned to task successfully', $task);
            return response()->json($response);
        } catch (\Exception $e) {
            Log::error('TaskController::assignDeveloper - Exception occurred', [
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString()
            ]);
            throw $e;
        }
    }

    /**
     * Move a task to another sprint
     */
    public function moveToSprint(Request $request, Task $task)
    {
        Log::info('TaskController::moveToSprint - Starting move task to sprint', [
            'taskId' => $task->Id ?? 'null',
            'request_data' => $request->all()
        ]);

        if (!$task) {
            Log::warning('TaskController::moveToSprint - Task not found');
            $this->auditService->logAction('UPDATE_FAILED', 'Task', 'Task not found');
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        try {
            $request->validate([
                'sprintId' => 'required|uuid|exists:Sprints,Id',
            ]);

            Log::info('TaskController::moveToSprint - Validation passed');

            $sprintId = $request->input('sprintId');
            $task->SprintId = $sprintId;
            $task->save();

            Log::info('TaskController::moveToSprint - Task moved successfully', [
                'taskId' => $task->Id,
                'sprintId' => $sprintId
            ]);

            $this->auditService->logAction('UPDATE_SUCCESS', 'Task', 'Moved task to sprint: ' . $task->Name);

            $response = ApiResponse::Success('Task moved to sprint successfully', $task);
            return response()->json($response);
        } catch (\Exception $e) {
            Log::error('TaskController::moveToSprint - Exception occurred', [
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString()
            ]);
            throw $e;
        }
    }

    /**
     * Get count of tasks in a sprint
     */
    public function countTasksInSprint(Sprint $sprint)
    {
        $counts = $sprint->tasks()
            ->with('taskStatus')
            ->get()
            ->groupBy('TaskStatusId')
            ->map(function ($tasks, $statusId) {
                $firstTask = $tasks->first();
                return [
                    'statusId' => $statusId,
                    'statusName' => $firstTask->taskStatus->Name,
                    'count' => $tasks->count()
                ];
            })
            ->values();

        $response = ApiResponse::Success('Task counts by status retrieved successfully', $counts);
        return response()->json($response);
    }

    /**
     * Remove the specified resource from storage.
     */
    public function destroy(Task $task)
    {
        Log::info('TaskController::destroy - Starting task deletion', ['taskId' => $task->Id ?? 'null']);

        if (!$task) {
            Log::warning('TaskController::destroy - Task not found');
            $this->auditService->logAction('DELETE_FAILED', 'Task', 'Task not found');
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        try {
            $taskName = $task->Name;
            $taskId = $task->Id;
            $task->delete();

            Log::info('TaskController::destroy - Task deleted successfully', [
                'taskId' => $taskId,
                'taskName' => $taskName
            ]);

            $this->auditService->logAction('DELETE_SUCCESS', 'Task', 'Deleted task: ' . $taskName);

            return response(status: ResponseAlias::HTTP_NO_CONTENT);
        } catch (\Exception $e) {
            Log::error('TaskController::destroy - Exception occurred', [
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString()
            ]);
            throw $e;
        }
    }

    /**
     * Get unassigned tasks in a project
     */
    public function getUnassignedTasksInProject(string $projectId)
    {
        Log::info('TaskController::getUnassignedTasksInProject - Starting', ['projectId' => $projectId]);

        if (Project::where('Id', $projectId)->doesntExist()) {
            Log::warning('TaskController::getUnassignedTasksInProject - Project not found');
            $response = ApiResponse::NotFound('Project not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        $tasks = Task::where('ProjectId', $projectId)
            ->whereNull('DeveloperId')
            ->get();

        Log::info('TaskController::getUnassignedTasksInProject - Retrieved tasks', ['count' => $tasks->count()]);

        if ($tasks->isEmpty()) {
            $response = ApiResponse::Success('No unassigned tasks found in this project', []);
            return response()->json($response);
        }

        $response = ApiResponse::Success('Unassigned tasks retrieved successfully', $tasks);
        return response()->json($response);
    }

    /**
     * Get unassigned tasks in sprint
     */
    public function getUnassignedTasksInSprint(string $sprintId)
    {
        Log::info('TaskController::getUnassignedTasksInSprint - Starting', ['sprintId' => $sprintId]);

        if (Sprint::where('Id', $sprintId)->doesntExist()) {
            Log::warning('TaskController::getUnassignedTasksInSprint - Sprint not found');
            $response = ApiResponse::NotFound('Sprint not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        $tasks = Task::where('SprintId', $sprintId)
            ->whereNull('DeveloperId')
            ->get();

        Log::info('TaskController::getUnassignedTasksInSprint - Retrieved tasks', ['count' => $tasks->count()]);

        if ($tasks->isEmpty()) {
            $response = ApiResponse::Success('No unassigned tasks found in this sprint', []);
            return response()->json($response);
        }

        $response = ApiResponse::Success('Unassigned tasks retrieved successfully', $tasks);
        return response()->json($response);
    }

    /**
     * Get tasks assigned to sprint and developer
     */
    public function getAssignedTasksInSprintAndDeveloper(string $sprintId)
    {
        Log::info('TaskController::getAssignedTasksInSprintAndDeveloper - Starting', ['sprintId' => $sprintId]);

        if (Sprint::where('Id', $sprintId)->doesntExist()) {
            Log::warning('TaskController::getAssignedTasksInSprintAndDeveloper - Sprint not found');
            $response = ApiResponse::NotFound('Sprint not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        $tasks = Task::where('SprintId', $sprintId)
            ->whereNotNull('DeveloperId')
            ->get();

        Log::info('TaskController::getAssignedTasksInSprintAndDeveloper - Retrieved tasks', ['count' => $tasks->count()]);

        if ($tasks->isEmpty()) {
            $response = ApiResponse::Success('No tasks found for this sprint and developer', []);
            return response()->json($response);
        }

        $response = ApiResponse::Success('Tasks retrieved successfully', $tasks);
        return response()->json($response);
    }

    /**
     * Get tasks assigned to developer
     */
    public function getTasksAssignedToDeveloper(string $developerId)
    {
        Log::info('TaskController::getTasksAssignedToDeveloper - Starting', ['developerId' => $developerId]);

        if (User::where('Id', $developerId)->doesntExist()) {
            Log::warning('TaskController::getTasksAssignedToDeveloper - Developer not found');
            $response = ApiResponse::NotFound('Developer not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        $tasks = Task::where('DeveloperId', $developerId)
            ->get();

        Log::info('TaskController::getTasksAssignedToDeveloper - Retrieved tasks', ['count' => $tasks->count()]);

        if ($tasks->isEmpty()) {
            $response = ApiResponse::Success('No tasks found for this developer', []);
            return response()->json($response);
        }

        $response = ApiResponse::Success('Tasks retrieved successfully', $tasks);
        return response()->json($response);
    }

    /**
     * Get tasks by manager id based on manager team
     * Gets tasks assigned to developers who are in a team managed by this manager
     */
    public function getTasksByManagerId(string $managerId)
    {
        Log::info('TaskController::getTasksByManagerId - Starting', ['managerId' => $managerId]);

        if (User::where('Id', $managerId)->doesntExist()) {
            Log::warning('TaskController::getTasksByManagerId - Manager not found');
            $response = ApiResponse::NotFound('Manager not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        $tasks = Task::whereHas('user.team', function ($query) use ($managerId) {
            $query->where('ManagerId', $managerId);
        })->get();

        Log::info('TaskController::getTasksByManagerId - Retrieved tasks', ['count' => $tasks->count()]);

        if ($tasks->isEmpty()) {
            $response = ApiResponse::Success('No tasks found for this manager', []);
            return response()->json($response);
        }

        $response = ApiResponse::Success('Tasks retrieved successfully', $tasks);
        return response()->json($response);
    }
}
