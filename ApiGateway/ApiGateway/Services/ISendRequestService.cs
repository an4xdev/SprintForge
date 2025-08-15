using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Services;

public interface ISendRequestService
{
    public Task<ActionResult<T>> SendRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        ServiceType serviceType,
        HttpContent? content = null,
        object? body = null);
    
}