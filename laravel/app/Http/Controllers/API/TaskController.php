<?php

namespace App\Http\Controllers\API;

use App\Http\Controllers\Controller;
use App\Http\Responses\ApiResponse;
use App\Models\Sprint;
use App\Models\Task;
use App\Models\TaskStatus;
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

        if ($request->has('task_status_id')) {
            $response = ApiResponse::BadRequest('Task status ID automatically assigned to `Created` status. Do not provide it in the request.');
            return response()->json($response, Response::HTTP_BAD_REQUEST);
        }

        $statusId = TaskStatus::where('Name', 'Created')->value('Id');
        if (!$statusId) {
            $response = ApiResponse::InternalError('Default task status "Created" not found');
            return response()->json($response, Response::HTTP_INTERNAL_SERVER_ERROR);
        }

        if ($request->has('developer_id') && $request->input('developer_id') !== null) {
            if (User::where('Id', $request->input('developer_id'))->doesntExist()) {
                $response = ApiResponse::BadRequest('Developer not found');
                return response()->json($response, Response::HTTP_BAD_REQUEST);
            }
        }

        if ($request->has('sprint_id') && $request->input('sprint_id') !== null) {
            if (Sprint::where('Id', $request->input('sprint_id'))->doesntExist()) {
                $response = ApiResponse::BadRequest('Sprint not found');
                return response()->json($response, Response::HTTP_BAD_REQUEST);
            }
        }

        $taskData = [
            'Id' => Str::uuid()->toString(),
            'Name' => $request->input('name'),
            'Description' => $request->input('description', null),
            'TaskTypeId' => $request->input('task_type_id'),
            'TaskStatusId' => $statusId,
            'DeveloperId' => $request->input('developer_id', null),
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

    // TODO: method for assigning a task to a developer by manager

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
            'name' => 'sometimes|required|string|max:255',
            'description' => 'sometimes|nullable|string|max:255',
            'task_type_id' => 'sometimes|required|integer|exists:TaskTypes,Id',
        ]);

        if ($request->has('developer_id')) {
            $response = ApiResponse::BadRequest('Cannot update developer_id directly. Use assign task to developer method.');
            return response()->json($response, Response::HTTP_BAD_REQUEST);
        }

        if ($request->has('sprint_id')) {
            $response = ApiResponse::BadRequest('Cannot update sprint_id directly. Use move task to another sprint method.');
            return response()->json($response, Response::HTTP_BAD_REQUEST);
        }

        $task->update($request->all());

        $response = ApiResponse::Success('Task updated successfully', $task);
        return response()->json($response);
    }

    /**
     * Assign a developer to a task
     */
    public function assignDeveloper(Request $request, Task $task)
    {
        if ($request->has('developer_id')) {

            if (!$task) {
                $response = ApiResponse::NotFound('Task not found');
                return response()->json($response, Response::HTTP_NOT_FOUND);
            }
            $request->validate([
                'developer_id' => 'required|uuid|exists:Users,Id',
            ]);

            $task->DeveloperId = $request->input('developer_id');
            $task->save();

            $response = ApiResponse::Success('Developer assigned to task successfully', $task);
            return response()->json($response);
        }

        return response()->json(
            ApiResponse::BadRequest('No developer_id provided'),
            Response::HTTP_BAD_REQUEST
        );
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
            'sprint_id' => 'required|uuid|exists:Sprints,Id',
        ]);

        $sprintId = $request->input('sprint_id');
        $task->SprintId = $sprintId;
        $task->save();

        $response = ApiResponse::Success('Task moved to sprint successfully', $task);
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
