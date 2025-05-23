<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;

/**
 * Class TaskStatus
 * 
 * @property int $Id
 * @property string $Name
 * 
 * @property Collection|Task[] $tasks
 *
 * @package App\Models
 */
class TaskStatus extends Model
{
	protected $table = 'TaskStatuses';
	protected $primaryKey = 'Id';
	public $timestamps = false;

	protected $fillable = [
		'Name'
	];

	public function tasks()
	{
		return $this->hasMany(Task::class, 'TaskStatusId');
	}
}
