<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Carbon\Carbon;
use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;

/**
 * Class User
 *
 * @property uuid $Id
 * @property string $Username
 * @property string $PasswordHash
 * @property string $Role
 * @property string|null $RefreshToken
 * @property Carbon|null $RefreshTokenExpiryTime
 * @property string|null $Avatar
 * @property string $PasswordSalt
 * @property uuid|null $TeamId
 * @property bool $NeedsPasswordChange
 *
 * @property Team|null $team
 * @property Collection|Task[] $tasks
 * @property Collection|Sprint[] $sprints
 *
 * @package App\Models
 */
class User extends Model
{
	protected $table = 'Users';
	protected $primaryKey = 'Id';
	public $incrementing = false;
	public $timestamps = false;
	public static $snakeAttributes = false;

	protected $casts = [
		'Id' => 'string',
		'RefreshTokenExpiryTime' => 'datetime',
		'TeamId' => 'string'
	];

	protected $hidden = [
		'PasswordHash',
		'RefreshToken',
		'RefreshTokenExpiryTime',
		'PasswordSalt'
	];

	protected $fillable = [
		'Username',
		'PasswordHash',
		'Role',
		'RefreshToken',
		'RefreshTokenExpiryTime',
		'Avatar',
		'PasswordSalt',
		'TeamId',
		'NeedsPasswordChange'
	];

	public function tasks(): User|\Illuminate\Database\Eloquent\Relations\HasMany
    {
		return $this->hasMany(Task::class, 'DeveloperId');
	}

	public function sprints(): User|\Illuminate\Database\Eloquent\Relations\HasMany
    {
		return $this->hasMany(Sprint::class, 'ManagerId');
	}
}
