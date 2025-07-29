namespace ApiGateway.Services;

public static class Utils
{
    public static string GetHost(this ServiceType serviceType)
    {
        return serviceType switch
        {
            ServiceType.AuthService => "http://auth:80/api",
            ServiceType.DatabaseManager => "http://db_manager:80/api",
            ServiceType.FastApiService => "http://fast_api:8000/api",
            ServiceType.SpringService => "http://spring:8080/api",
            ServiceType.LaravelService => "http://nginx:80/api",
            ServiceType.ExpressService => "http://js_express:6713/api",
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null)
        };
    }
}