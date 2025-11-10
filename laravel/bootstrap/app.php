<?php

use Illuminate\Foundation\Application;
use Illuminate\Foundation\Configuration\Exceptions;
use Illuminate\Foundation\Configuration\Middleware;
use Illuminate\Support\Facades\Log;

return Application::configure(basePath: dirname(__DIR__))
    ->withRouting(
        web: __DIR__ . '/../routes/web.php',
        commands: __DIR__ . '/../routes/console.php',
        api: __DIR__ . '/../routes/api.php',
        health: '/up',
    )
    ->withMiddleware(function (Middleware $middleware) {
        // https://medium.com/@agitari65/disabling-csrf-protection-in-laravel-11-a-step-by-step-guide-8b41216ee571
        //
        // $middleware->statefulApi();
        $middleware->validateCsrfTokens(
            except: ['*']
        );
        
        $middleware->append(\App\Http\Middleware\LogRequests::class);
    })
    ->withExceptions(function (Exceptions $exceptions) {
        $exceptions->render(function (\Throwable $e, $request) {
            if ($e instanceof \Illuminate\Validation\ValidationException) {
                Log::warning('Validation failed:', [
                    'errors' => $e->errors(),
                    'url' => $request->fullUrl(),
                    'method' => $request->method(),
                    'input' => $request->except(['password', 'password_confirmation']),
                ]);

                if ($request->is('api/*') || $request->expectsJson()) {
                    return response()->json([
                        'success' => false,
                        'message' => 'Validation failed',
                        'errors' => $e->errors(),
                    ], 400);
                }
            }

            if ($e instanceof \Illuminate\Database\Eloquent\ModelNotFoundException) {
                Log::warning('Model not found:', [
                    'model' => $e->getModel(),
                    'url' => $request->fullUrl(),
                    'method' => $request->method(),
                ]);

                if ($request->is('api/*') || $request->expectsJson()) {
                    $modelName = class_basename($e->getModel());
                    return response()->json([
                        'success' => false,
                        'message' => "{$modelName} not found",
                    ], 404);
                }
            }

            if ($e instanceof \Symfony\Component\HttpKernel\Exception\NotFoundHttpException) {
                Log::warning('Resource not found:', [
                    'url' => $request->fullUrl(),
                    'method' => $request->method(),
                ]);

                if ($request->is('api/*') || $request->expectsJson()) {
                    return response()->json([
                        'success' => false,
                        'message' => 'Resource not found',
                    ], 404);
                }
            }

            Log::error('Exception caught:', [
                'type' => get_class($e),
                'message' => $e->getMessage(),
                'file' => $e->getFile(),
                'line' => $e->getLine(),
                'trace' => $e->getTraceAsString(),
                'url' => $request->fullUrl(),
                'method' => $request->method(),
                'input' => $request->except(['password', 'password_confirmation']),
                'headers' => $request->headers->all(),
            ]);

            if ($request->is('api/*') || $request->expectsJson()) {
                return response()->json([
                    'success' => false,
                    'message' => 'Server Error: ' . $e->getMessage(),
                    'error' => [
                        'type' => get_class($e),
                        'message' => $e->getMessage(),
                        'file' => $e->getFile(),
                        'line' => $e->getLine(),
                        'trace' => config('app.debug') ? $e->getTraceAsString() : null,
                    ]
                ], 500);
            }
        });
    })->create();
