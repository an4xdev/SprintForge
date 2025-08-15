using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
    private static ILogger? _logger;
    
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    public static T? GetResultData<T>(ActionResult<ApiResponse<T>> result) where T : struct
    {
        _logger?.LogInformation("[DEBUG] GetResultData called for type {Type}", typeof(T).Name);
        
        if (result.Value != null)
        {
            _logger?.LogInformation("[DEBUG] Using result.Value.Data");
            return result.Value.Data;
        }
            
        if (result.Result is OkObjectResult { Value: ApiResponse<T> apiResponse })
        {
            _logger?.LogInformation("[DEBUG] Using OkObjectResult apiResponse.Data");
            return apiResponse.Data;
        }
        
        _logger?.LogInformation("[DEBUG] Returning null for GetResultData");
        return null;
    }

    public static T? GetResultDataReference<T>(ActionResult<ApiResponse<T>> result) where T : class
    {
        _logger?.LogInformation("[DEBUG] GetResultDataReference called for type {Type}", typeof(T).Name);
        _logger?.LogInformation("[DEBUG] result.Value is null: {IsNull}", result.Value == null);
        _logger?.LogInformation("[DEBUG] result.Result type: {Type}", result.Result?.GetType().Name ?? "null");
        
        if (result.Value != null)
        {
            _logger?.LogInformation("[DEBUG] Using result.Value.Data");
            return result.Value.Data;
        }
            
        if (result.Result is OkObjectResult { Value: ApiResponse<T> apiResponse })
        {
            _logger?.LogInformation("[DEBUG] Using OkObjectResult apiResponse.Data");
            return apiResponse.Data;
        }
        
        if (result.Result is ObjectResult objectResult)
        {
            _logger?.LogInformation("[DEBUG] ObjectResult StatusCode: {StatusCode}", objectResult.StatusCode);
            _logger?.LogInformation("[DEBUG] ObjectResult Value type: {Type}", objectResult.Value?.GetType().Name ?? "null");
            
            if (objectResult.Value is ApiResponse<T> objApiResponse)
            {
                _logger?.LogInformation("[DEBUG] Using ObjectResult apiResponse.Data");
                return objApiResponse.Data;
            }
        }
            
        _logger?.LogInformation("[DEBUG] Returning null");
        return null;
    }
}