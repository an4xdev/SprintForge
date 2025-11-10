<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

/**
 * Class Team
 *
 * @property uuid $Id
 * @property string $Name
 * @property uuid $ManagerId
 * @property uuid $ProjectId
 *
 * @property User $user
 * @property Project $project
 * @property Collection|Sprint[] $sprints
 * @property Collection|User[] $users
 *
 * @package App\Models
 */
class Team extends Model
{
	protected $table = 'Teams';
	protected $primaryKey = 'Id';
	public $incrementing = false;
	public $timestamps = false;
	public static $snakeAttributes = false;

	protected $casts = [
		'Id' => 'string',
		'ManagerId' => 'string',
		'ProjectId' => 'string'
	];

	protected $fillable = [
		'Name',
		'ManagerId',
		'ProjectId'
	];

	public function user(): BelongsTo
    {
		return $this->belongsTo(User::class, 'ManagerId');
	}

    public function users()
	{
		return $this->hasMany(User::class, 'TeamId');
	}
}
