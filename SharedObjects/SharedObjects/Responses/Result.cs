namespace SharedObjects.Responses;

public class Result<T>
{
    public T? Value { get; }
    public string Message { get; }
    public ResultStatus Status { get; }

    private Result(T? value, string message, ResultStatus status)
    {
        Value = value;
        Message = message;
        Status = status;
    }

    public static Result<T> Success(T value, string message) => new(value, message, ResultStatus.Ok);

    public static Result<T> Created(T value, string message) => new(value, message, ResultStatus.Created);

    public static Result<T?> NoContent() => new(default, string.Empty, ResultStatus.NoContent);

    public static Result<T> BadRequest(string message = "Bad Request") => new(default, message, ResultStatus.BadRequest);

    public static Result<T> NotFound(string message = "Resource not found") => new(default, message, ResultStatus.NotFound);

    public static Result<T> InternalError(string message = "Internal Server Error") =>
        new(default, message, ResultStatus.InternalError);
}

public enum ResultStatus
{
    Ok,
    Created,
    NoContent,
    BadRequest,
    NotFound,
    InternalError
}