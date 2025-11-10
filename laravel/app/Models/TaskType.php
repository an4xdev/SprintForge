<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

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

	public function tasks(): TaskType|HasMany
    {
		return $this->hasMany(Task::class, 'TaskTypeId');
	}
}
