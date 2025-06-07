<?php

use Illuminate\Support\Facades\Route;

Route::resource('taskStatuses', 'App\Http\Controllers\API\TaskStatusController');

Route::resource('tasks', 'App\Http\Controllers\API\TaskController');
