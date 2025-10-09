<?php

namespace App\Http\Controllers;

use App\Services\AuditService;
use Exception;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\DB;

class HealthController extends Controller
{
    private AuditService $auditService;

    public function __construct(AuditService $auditService)
    {
        $this->auditService = $auditService;
    }

    public function check(): JsonResponse
    {
        $health = [
            'status' => 'healthy',
            'checks' => [
                'database' => $this->checkDatabase(),
                'rabbitmq' => $this->checkRabbitMQ()
            ]
        ];

        $allHealthy = collect($health['checks'])->every(function ($check) {
            return $check['status'] === 'up';
        });

        if (!$allHealthy) {
            $health['status'] = 'unhealthy';
            return response()->json($health, 503);
        }

        return response()->json($health, 200);
    }

    private function checkDatabase(): array
    {
        try {
            DB::connection()->getPdo();
            return [
                'status' => 'up',
                'message' => 'Database connection is healthy'
            ];
        } catch (Exception $e) {
            return [
                'status' => 'down',
                'message' => 'Database connection failed: ' . $e->getMessage()
            ];
        }
    }

    private function checkRabbitMQ(): array
    {
        try {
            $testAuditService = new AuditService();
            if ($testAuditService) {
                return [
                    'status' => 'up',
                    'message' => 'RabbitMQ connection is healthy'
                ];
            }
            return [
                'status' => 'down',
                'message' => 'RabbitMQ connection failed'
            ];
        } catch (Exception $e) {
            return [
                'status' => 'down',
                'message' => 'RabbitMQ connection failed: ' . $e->getMessage()
            ];
        }
    }
}
