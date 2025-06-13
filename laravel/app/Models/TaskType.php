<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;

/**
 * Class TaskType
 * 
 * @property int $Id
 * @property string $Name
 * 
 * @property Collection|Task[] $tasks
 *
 * @package App\Models
 */
class TaskType extends Model
{
	protected $table = 'TaskTypes';
	protected $primaryKey = 'Id';
	public $timestamps = false;
	public static $snakeAttributes = false;

	protected $fillable = [
		'Name'
	];

	public function tasks()
	{
		return $this->hasMany(Task::class, 'TaskTypeId');
	}
}
