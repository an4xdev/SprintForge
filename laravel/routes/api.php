<?php

use Illuminate\Support\Facades\Route;

Route::resource('task-status', 'App\Http\Controllers\API\TaskStatusController');

Route::resource('task', 'App\Http\Controllers\API\TaskController');
