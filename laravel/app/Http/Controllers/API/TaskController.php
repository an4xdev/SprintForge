<?php

namespace App\Http\Controllers\API;

use App\Http\Controllers\Controller;
use App\Http\Responses\ApiResponse;
use App\Models\Sprint;
use App\Models\Task;
use App\Models\TaskStatus;
use App\Models\TaskType;
use App\Models\User;
use Illuminate\Http\Request;
use Illuminate\Http\Response;
use Illuminate\Support\Str;

class TaskController extends Controller
{
    /**
     * Display a listing of the resource.
     */
    public function index()
    {
        $tasks = Task::all();
        $response = ApiResponse::Success('Tasks retrieved successfully', $tasks);
        return response()->json($response);
    }

    /**
     * Store a newly created resource in storage.
     */
    public function store(Request $request)
    {
        $request->validate([
            'name' => 'required|string|max:255',
            'description' => 'nullable|string|max:255',
        ]);

        if ($request->has('taskStatusId')) {
            $response = ApiResponse::BadRequest('Task status ID automatically assigned to `Created` status. Do not provide it in the request.');
            return response()->json($response, Response::HTTP_BAD_REQUEST);
        }

        $statusId = TaskStatus::where('Name', 'Created')->value('Id');
        if (!$statusId) {
            $response = ApiResponse::InternalError('Default task status "Created" not found');
            return response()->json($response, Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        if ($request->has('developerId') && $request->input('developerId') !== null) {
            if (User::where('Id', $request->input('developerId'))->doesntExist()) {
                $response = ApiResponse::BadRequest('Developer not found');
                return response()->json($response, Response::HTTP_BAD_REQUEST);
            }
        }

        if ($request->has('taskTypeId') && $request->input('taskTypeId') !== null) {
            if (!TaskType::where('Id', $request->input('taskTypeId'))->exists()) {
                $response = ApiResponse::BadRequest('Task Type not found');
                return response()->json($response, Response::HTTP_BAD_REQUEST);
            }
        }

        if ($request->has('sprintId') && $request->input('sprintId') !== null) {
            if (Sprint::where('Id', $request->input('sprintId'))->doesntExist()) {
                $response = ApiResponse::BadRequest('Sprint not found');
                return response()->json($response, Response::HTTP_BAD_REQUEST);
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

        $task = Task::create($taskData);
        $response = ApiResponse::Created('Task created successfully', $task);
        return response()->json($response, Response::HTTP_CREATED);
    }

    /**
     * Display the specified resource.
     */
    public function show(Task $task)
    {
        if (!$task) {
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, Response::HTTP_NOT_FOUND);
        }
        $response = ApiResponse::Success('Task retrieved successfully', $task);
        return response()->json($response);
    }

    /**
     * Update the specified resource in storage.
     */
    public function update(Request $request, Task $task)
    {
        if (!$task) {
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, Response::HTTP_NOT_FOUND);
        }
        $request->validate([
            'name' => 'nullable|string|max:255',
            'description' => 'nullable|string|max:255',
            'taskTypeId' => 'nullable|integer|exists:TaskTypes,Id',
        ]);

        if ($request->has('developerId')) {
            $response = ApiResponse::BadRequest('Cannot update developerId directly. Use assign task to developer method.');
            return response()->json($response, Response::HTTP_BAD_REQUEST);
        }

        if ($request->has('sprintId')) {
            $response = ApiResponse::BadRequest('Cannot update sprintId directly. Use move task to another sprint method.');
            return response()->json($response, Response::HTTP_BAD_REQUEST);
        }

        $task->update(
            [
                'Name' => $request->input('name', $task->Name),
                'Description' => $request->input('description', $task->Description),
                'TaskTypeId' => $request->input('taskTypeId', $task->TaskTypeId),
            ]
        );

        $response = ApiResponse::Success('Task updated successfully', $task);
        return response()->json($response);
    }

    /**
     * Assign a developer to a task
     */
    public function assignDeveloper(Request $request, Task $task)
    {
        if (!$task) {
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, Response::HTTP_NOT_FOUND);
        }

        $request->validate([
            'developerId' => 'required|uuid|exists:Users,Id',
        ]);

        $task->DeveloperId = $request->input('developerId');
        $task->save();

        $response = ApiResponse::Success('Developer assigned to task successfully', $task);
        return response()->json($response);
    }

    /**
     * Move a task to another sprint
     */
    public function moveToSprint(Request $request, Task $task)
    {
        if (!$task) {
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, Response::HTTP_NOT_FOUND);
        }

        $request->validate([
            'sprintId' => 'required|uuid|exists:Sprints,Id',
        ]);

        $sprintId = $request->input('sprintId');
        $task->SprintId = $sprintId;
        $task->save();

        $response = ApiResponse::Success('Task moved to sprint successfully', $task);
        return response()->json($response);
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
        if (!$task) {
            $response = ApiResponse::NotFound('Task not found');
            return response()->json($response, Response::HTTP_NOT_FOUND);
        }
        $task->delete();
        return response(status: Response::HTTP_NO_CONTENT);
    }
}
