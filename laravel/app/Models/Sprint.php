<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Carbon\Carbon;
use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;

/**
 * Class Sprint
 * 
 * @property uuid $Id
 * @property string $Name
 * @property Carbon $StartDate
 * @property Carbon $EndDate
 * @property uuid $ManagerId
 * @property uuid|null $ProjectId
 * @property uuid $TeamId
 * 
 * @property User $user
 * @property Project|null $project
 * @property Team $team
 * @property Collection|Task[] $tasks
 *
 * @package App\Models
 */
class Sprint extends Model
{
	protected $keyType = 'string';
	protected $table = 'Sprints';
	protected $primaryKey = 'Id';
	public $incrementing = false;
	public $timestamps = false;

	protected $casts = [
		'Id' => 'string',
		'StartDate' => 'datetime',
		'EndDate' => 'datetime',
		'ManagerId' => 'string',
		'ProjectId' => 'string',
		'TeamId' => 'string'
	];

	protected $fillable = [
		'Name',
		'StartDate',
		'EndDate',
		'ManagerId',
		'ProjectId',
		'TeamId'
	];

	public function user()
	{
		return $this->belongsTo(User::class, 'ManagerId');
	}

	public function tasks()
	{
		return $this->hasMany(Task::class, 'SprintId');
	}
}
