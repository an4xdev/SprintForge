<?php

/**
 * Created by Reliese Model.
 */

namespace App\Models;

use Carbon\Carbon;
use Illuminate\Database\Eloquent\Collection;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

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
 * @property bool $NeedResetPassword
 * @property string $Email
 * @property string $FirstName
 * @property string $LastName
 *
 * @property Team|null $team
 * @property Collection|Task[] $tasks
 * @property Collection|Sprint[] $sprints
 * @property Collection|Team[] $teams
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
		'TeamId' => 'string',
		'NeedResetPassword' => 'bool'
	];

	protected $hidden = [
		'PasswordHash',
		'RefreshToken',
		'RefreshTokenExpiryTime',
		'PasswordSalt',
		'NeedResetPassword'
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
		'NeedResetPassword',
		'Email',
		'FirstName',
		'LastName'
	];

	public function team(): BelongsTo
    {
		return $this->belongsTo(Team::class, 'TeamId');
	}

	public function tasks(): User|HasMany
    {
		return $this->hasMany(Task::class, 'DeveloperId');
	}

	public function sprints(): User|HasMany
    {
		return $this->hasMany(Sprint::class, 'ManagerId');
	}

	public function teams(): User|HasMany
    {
		return $this->hasMany(Team::class, 'ManagerId');
	}
}
