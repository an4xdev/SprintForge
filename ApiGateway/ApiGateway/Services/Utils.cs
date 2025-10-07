using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Services;

public static class Utils
{
    public static string GetHost(this ServiceType serviceType)
    {
        return serviceType switch
        {
            // ServiceType.AuthService => "http://localhost:8080/api",
            ServiceType.AuthService => "http://auth:80/api",
            ServiceType.DatabaseManager => "http://db_manager:80/api",
            ServiceType.FastApiService => "http://fast_api:8000/api",
            // ServiceType.SpringService => "http://localhost:2020/api",
            ServiceType.SpringService => "http://spring:8080/api",
            ServiceType.LaravelService => "http://nginx:80/api",
            // ServiceType.ExpressService => "http://localhost:6713/api",
            ServiceType.ExpressService => "http://js_express:6713/api",
            ServiceType.ReportsService => "http://reports:8080/api",
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null)
        };
    }

    public static bool IsSuccessResult<T>(ActionResult<T> result)
    {
        if (result.Result is OkObjectResult)
            return true;

        if (result.Value != null)
            return true;

        if (result.Result is ObjectResult objectResult)
        {
            return objectResult.StatusCode is 200 or null;
        }
        
        return false;
    }
}