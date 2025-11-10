<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

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
 * @property Collection|TaskHistory[] $taskHistories
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

	public function taskType(): BelongsTo
    {
		return $this->belongsTo(TaskType::class, 'TaskTypeId');
	}

	public function taskStatus(): BelongsTo
    {
		return $this->belongsTo(TaskStatus::class, 'TaskStatusId');
	}

	public function user(): BelongsTo
    {
		return $this->belongsTo(User::class, 'DeveloperId');
	}

	public function sprint(): BelongsTo
    {
		return $this->belongsTo(Sprint::class, 'SprintId');
	}
}
