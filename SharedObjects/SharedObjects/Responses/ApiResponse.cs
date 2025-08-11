using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace SharedObjects.Responses;

public class ApiResponse<T>
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public static class ApiResponseExtensions
{
    public static T? GetResultData<T>(ActionResult<ApiResponse<T>> result) where T : struct
    {
        if (result.Value != null)
            return result.Value.Data;
            
        if (result.Result is OkObjectResult { Value: ApiResponse<T> apiResponse })
            return apiResponse.Data;
            
        return null;
    }

    public static T? GetResultDataReference<T>(ActionResult<ApiResponse<T>> result) where T : class
    {
        return result.Result is OkObjectResult { Value: ApiResponse<T> apiResponse } ? apiResponse.Data : null;
    }
}