<?php

namespace App\Http\Controllers\API;

use App\Http\Controllers\Controller;
use App\Http\Responses\ApiResponse;
use App\Models\Task;
use App\Models\TaskStatus;
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

        $statusId = TaskStatus::where('Name', 'Created')->value('Id');

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
        ]);
        $task->update($request->all());

        $response = ApiResponse::Success('Task updated successfully', $task);
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
