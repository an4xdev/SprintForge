<?php

namespace App\Http\Controllers\API;

use App\Http\Controllers\Controller;

use App\Models\TaskStatus;
use Illuminate\Http\Request;
use App\Http\Responses\ApiResponse;
use Illuminate\Http\Response;
use Illuminate\Support\Facades\Log;
use Symfony\Component\HttpFoundation\Response as ResponseAlias;
use App\Services\AuditService;

class TaskStatusController extends Controller
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
        Log::info('TaskStatusController::index - Retrieving all task statuses');
        $taskStatuses = TaskStatus::all();
        Log::info('TaskStatusController::index - Retrieved task statuses', ['count' => $taskStatuses->count()]);

        if($taskStatuses->isEmpty()) {
            $response = ApiResponse::Success('No task statuses found', []);
        }
        else
        {
            $response = ApiResponse::Success('Task Statuses retrieved successfully', $taskStatuses);
        }
        return response()->json($response);
    }

    /**
     * Store a newly created resource in storage.
     */
    public function store(Request $request)
    {
        Log::info('TaskStatusController::store - Starting task status creation', [
            'request_data' => $request->all()
        ]);

        try {
            $request->validate([
                'name' => 'required|string|max:255',
            ]);

            Log::info('TaskStatusController::store - Validation passed');

            $taskStatus = TaskStatus::create([
                'Name' => $request->input('name'),
            ]);

            Log::info('TaskStatusController::store - Task status created successfully', [
                'taskStatusId' => $taskStatus->Id
            ]);

            $this->auditService->logAction('CREATE_SUCCESS', 'TaskStatus', 'Created new task status: ' . $request->input('name'));
            $response = ApiResponse::Created('Task Status created successfully', $taskStatus);
            return response()->json($response, ResponseAlias::HTTP_CREATED);
        } catch (\Exception $e) {
            Log::error('TaskStatusController::store - Exception occurred', [
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
    public function show(TaskStatus $taskStatus)
    {
        Log::info('TaskStatusController::show - Retrieving task status', [
            'taskStatusId' => $taskStatus->Id ?? 'null'
        ]);

        if (!$taskStatus) {
            Log::warning('TaskStatusController::show - Task Status not found');
            $response = ApiResponse::NotFound('Task Status not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        Log::info('TaskStatusController::show - Task status retrieved successfully', [
            'taskStatusId' => $taskStatus->Id
        ]);

        $response = ApiResponse::Success('Task Status retrieved successfully', $taskStatus);
        return response()->json($response);
    }

    /**
     * Update the specified resource in storage.
     */
    public function update(Request $request, TaskStatus $taskStatus)
    {
        Log::info('TaskStatusController::update - Starting task status update', [
            'taskStatusId' => $taskStatus->Id ?? 'null',
            'request_data' => $request->all()
        ]);

        if (!$taskStatus) {
            Log::warning('TaskStatusController::update - Task Status not found');
            $this->auditService->logAction('UPDATE_FAILED', 'TaskStatus', 'Task Status not found');
            $response = ApiResponse::BadRequest('Task Status not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        try {
            $request->validate([
                'name' => 'required|string|max:255',
            ]);

            Log::info('TaskStatusController::update - Validation passed');

            $taskStatus->update([
                'Name' => $request->input('name', $taskStatus->Name),
            ]);

            Log::info('TaskStatusController::update - Task status updated successfully', [
                'taskStatusId' => $taskStatus->Id
            ]);

            $response = ApiResponse::Success('Task Status updated successfully', $taskStatus);
            return response()->json($response);
        } catch (\Exception $e) {
            Log::error('TaskStatusController::update - Exception occurred', [
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString()
            ]);
            throw $e;
        }
    }

    /**
     * Remove the specified resource from storage.
     */
    public function destroy(TaskStatus $taskStatus)
    {
        Log::info('TaskStatusController::destroy - Starting task status deletion', [
            'taskStatusId' => $taskStatus->Id ?? 'null'
        ]);

        if (!$taskStatus) {
            Log::warning('TaskStatusController::destroy - Task Status not found');
            $this->auditService->logAction('DELETE_FAILED', 'TaskStatus', 'Task Status not found');
            $response = ApiResponse::NotFound('Task Status not found');
            return response()->json($response, ResponseAlias::HTTP_NOT_FOUND);
        }

        try {
            $taskStatusName = $taskStatus->Name;
            $taskStatusId = $taskStatus->Id;
            $taskStatus->delete();

            Log::info('TaskStatusController::destroy - Task status deleted successfully', [
                'taskStatusId' => $taskStatusId,
                'taskStatusName' => $taskStatusName
            ]);

            $this->auditService->logAction('DELETE_SUCCESS', 'TaskStatus', 'Deleted task status: ' . $taskStatusName);
            return response(status: Response::HTTP_NO_CONTENT);
        } catch (\Exception $e) {
            Log::error('TaskStatusController::destroy - Exception occurred', [
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString()
            ]);
            throw $e;
        }
    }
}
