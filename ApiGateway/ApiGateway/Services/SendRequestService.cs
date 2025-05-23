using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace ApiGateway.Services;

public class SendRequestService(IHttpClientFactory httpClientFactory, HybridCache hybridCache) : ISendRequestService
{
    private HttpClient GetHttpClient() => httpClientFactory.CreateClient();

    private static string CombinePath(ServiceType serviceType, string endpoint) => $"{serviceType.GetHost()}{endpoint}";

    public async Task<ActionResult<T>> SendRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        ServiceType serviceType,
        HttpContent? content = null,
        object? body = null)
    {
        try
        {
            var fullUrl = CombinePath(serviceType, endpoint);
            if (method == HttpMethod.Get)
            {
                return await hybridCache.GetOrCreateAsync(
                    key: fullUrl,
                    factory: async (cancellationToken) =>
                    {
                        var httpClient = GetHttpClient();
                        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                        var response = await httpClient.SendAsync(requestMessage, cancellationToken);

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken);
                            return new ObjectResult(errorContent)
                            {
                                StatusCode = (int)response.StatusCode
                            };
                        }

                        var responseData = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
                        return new OkObjectResult(responseData);
                    });
            }
            var httpClient = GetHttpClient();

            using var requestMessage = new HttpRequestMessage(method, fullUrl);

            if (content != null)
            {
                requestMessage.Content = content;
            }
            else if (body != null)
            {
                if (method == HttpMethod.Get || method == HttpMethod.Delete)
                {
                    return new ObjectResult("Request body not allowed for GET or DELETE methods.") { StatusCode = 400 };
                }
                var co = JsonContent.Create(body);
                Console.WriteLine($"[DEBUG]: SendRequestService to JSON: {co.Value}");
                requestMessage.Content = JsonContent.Create(body);
            }

            var response = await httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadFromJsonAsync<dynamic>();
                return new ObjectResult(errorContent)
                    { StatusCode = (int)response.StatusCode };
            }

            var responseData = await response.Content.ReadFromJsonAsync<T>();

            return new OkObjectResult(responseData);
        }
        catch (HttpRequestException httpEx)
        {
            return new ObjectResult($"An error occurred while sending the request: {httpEx.Message}")
                { StatusCode = 500 };
        }
        catch (Exception ex)
        {
            return new ObjectResult($"An unexpected error occurred: {ex.Message}")
                { StatusCode = 500 };
        }
    }
}