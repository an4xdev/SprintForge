using Microsoft.AspNetCore.Mvc;

namespace SharedObjects.Responses;

public static class ResultExtensions
{
    public static ActionResult<ApiResponse<T>> ToActionResult<T>(this Result<T> result, string? location = null)
    {
        var response = new ApiResponse<T?>
        {
            Data = result.Value,
            Message = result.Message,
        };

        return result.Status switch
        {
            ResultStatus.Ok => new OkObjectResult(response),
            ResultStatus.Created => new CreatedResult(location ?? string.Empty, response),
            ResultStatus.NoContent => new NoContentResult(),
            ResultStatus.NotFound => new NotFoundObjectResult(response),
            ResultStatus.BadRequest => new BadRequestObjectResult(response),
            _ => new ObjectResult(response) { StatusCode = 500 }
        };
    }
}