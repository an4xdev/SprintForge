<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;

/**
 * Class Task
 *
 * @property uuid $Id
 * @property string $Name
 * @property string|null $Description
 * @property int $TaskTypeId
 * @property int $TaskStatusId
 * @property uuid|null $DeveloperId
 * @property uuid|null $SprintId
 * @property uuid|null $ProjectId
 *
 * @property TaskType $taskType
 * @property TaskStatus $taskStatus
 * @property User|null $user
 * @property Sprint|null $sprint
 * @property Project|null $project
 *
 * @package App\Models
 */
class Task extends Model
{
	protected $table = 'Tasks';
	protected $primaryKey = 'Id';
	public $incrementing = false;
	public $timestamps = false;
	public static $snakeAttributes = false;

	protected $casts = [
		'Id' => 'string',
		'TaskTypeId' => 'int',
		'TaskStatusId' => 'int',
		'DeveloperId' => 'string',
		'SprintId' => 'string'
	];

	protected $fillable = [
		'Id',
		'Name',
		'Description',
		'TaskTypeId',
		'TaskStatusId',
		'DeveloperId',
		'SprintId',
		'ProjectId'
	];

	public function taskType(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
		return $this->belongsTo(TaskType::class, 'TaskTypeId');
	}

	public function taskStatus(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
		return $this->belongsTo(TaskStatus::class, 'TaskStatusId');
	}

	public function user(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
		return $this->belongsTo(User::class, 'DeveloperId');
	}

	public function sprint(): \Illuminate\Database\Eloquent\Relations\BelongsTo
    {
		return $this->belongsTo(Sprint::class, 'SprintId');
	}
}
