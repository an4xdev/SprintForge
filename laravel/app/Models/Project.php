<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

/**
 * Class Project
 *
 * @property uuid $Id
 * @property string $Name
 * @property string|null $Description
 *
 * @package App\Models
 */
class Project extends Model
{
	protected $table = 'Projects';
	protected $primaryKey = 'Id';
	public $incrementing = false;
	public $timestamps = false;
	public static $snakeAttributes = false;

	protected $casts = [
		'Id' => 'string',
	];

	protected $fillable = [
		'Id',
		'Name',
		'Description'
	];
}
