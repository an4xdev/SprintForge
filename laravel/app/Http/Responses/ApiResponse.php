<?php

namespace App\Http\Responses;

class ApiResponse implements \JsonSerializable
{
    /**
     * @var string
     */
    protected string $message;

    /**
     * @var mixed|null
     */
    protected mixed $data;

    private function __construct(string $message, mixed $data = null)
    {
        $this->message = $message;
        $this->data = $data;
    }

    public static function Success(string $message, mixed $data): ApiResponse
    {
        return new ApiResponse($message, $data);
    }

    public static function Created(string $message, mixed $data): ApiResponse
    {
        return new ApiResponse($message, $data);
    }

    public static function NotFound(string $message): ApiResponse
    {
        return new ApiResponse($message, null);
    }

    public static function BadRequest(string $message): ApiResponse
    {
        return new ApiResponse($message, null);
    }

    public static function InternalError(string $message): ApiResponse
    {
        return new ApiResponse($message, null);
    }

    public function jsonSerialize(): array
    {
        return [
            'message' => $this->message,
            'data' => $this->data,
        ];
    }
}
