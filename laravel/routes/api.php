<?php

use Illuminate\Support\Facades\Route;

Route::resource('taskStatuses', 'App\Http\Controllers\API\TaskStatusController');

Route::resource('tasks', 'App\Http\Controllers\API\TaskController');

Route::patch('tasks/{task}/assign-developer', 'App\Http\Controllers\API\TaskController@assignDeveloper');
Route::patch('tasks/{task}/move-to-sprint', 'App\Http\Controllers\API\TaskController@moveToSprint');
Route::get('sprints/{sprint}/tasks/count', 'App\Http\Controllers\API\TaskController@countTasksInSprint');
Route::get('tasks/unassigned/project/{projectId}', 'App\Http\Controllers\API\TaskController@getUnassignedTasksInProject');
Route::get('tasks/unassigned/sprint/{sprintId}', 'App\Http\Controllers\API\TaskController@getUnassignedTasksInSprint');
Route::get('tasks/assigned/sprint/{sprintId}', 'App\Http\Controllers\API\TaskController@getAssignedTasksInSprintAndDeveloper');
Route::get('tasks/developer/{developerId}', 'App\Http\Controllers\API\TaskController@getTasksAssignedToDeveloper');
