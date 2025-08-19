using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using StackExchange.Redis;

namespace ApiGateway.Services;

public class SendRequestService(
    IHttpClientFactory httpClientFactory,
    HybridCache hybridCache,
    ILogger<SendRequestService> logger,
    IDatabase redisDb)
    : ISendRequestService
{
    private HttpClient GetHttpClient() => httpClientFactory.CreateClient();

    private static string CombinePath(ServiceType serviceType, string endpoint)
        => $"{serviceType.GetHost()}{endpoint}";

    private static string GetCacheGroupKey(ServiceType serviceType, string group)
        => $"cache_keys:{serviceType}:{group}";

    public async Task<ActionResult<T>> SendRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        ServiceType serviceType,
        HttpContent? content = null,
        object? body = null)
    {
        logger.LogInformation("[INFO]: Sending request to {S}", endpoint);
        logger.LogInformation("[INFO]: Method: {Method}", method);
        logger.LogInformation("[INFO]: Service: {Service}", serviceType);

        try
        {
            var fullUrl = CombinePath(serviceType, endpoint);

            if (method == HttpMethod.Get)
            {
                try
                {
                    var cachedData = await hybridCache.GetOrCreateAsync(
                        key: fullUrl,
                        factory: async cancellationToken =>
                        {
                            var httpClient = GetHttpClient();
                            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                            var response = await httpClient.SendAsync(requestMessage, cancellationToken);

                            if (!response.IsSuccessStatusCode)
                            {
                                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
                            }

                            var responseData = await response.Content.ReadFromJsonAsync<T>(cancellationToken);

                            var group = GetGroupFromEndpoint(endpoint);
                            if (group != null)
                            {
                                await redisDb.SetAddAsync(GetCacheGroupKey(serviceType, group), fullUrl);
                            }

                            return responseData;
                        });

                    return new OkObjectResult(cachedData);
                }
                catch (HttpRequestException)
                {
                    var httpClientNonOk = GetHttpClient();
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                    var response = await httpClientNonOk.SendAsync(requestMessage);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadFromJsonAsync<dynamic>();
                        return new ObjectResult(errorContent)
                        { StatusCode = (int)response.StatusCode };
                    }

                    var responseData = await response.Content.ReadFromJsonAsync<T>();
                    return new OkObjectResult(responseData);
                }
            }

            if (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete)
            {
                var group = GetGroupFromEndpoint(endpoint);
                if (group != null)
                {
                    await InvalidateCacheGroupAsync(serviceType, group);
                }
            }

            var httpClient = GetHttpClient();
            using var requestMessageNonGet = new HttpRequestMessage(method, fullUrl);

            if (content != null)
            {
                requestMessageNonGet.Content = content;
            }
            else if (body != null)
            {
                if (method == HttpMethod.Get || method == HttpMethod.Delete)
                {
                    return new ObjectResult("Request body not allowed for GET or DELETE methods.") { StatusCode = 400 };
                }

                var serializedBody = JsonSerializer.Serialize(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                logger.LogInformation("[DEBUG]: SendRequestService to JSON: {SerializedBody}", serializedBody);
                requestMessageNonGet.Content =
                    new StringContent(serializedBody, Encoding.UTF8, "application/json");
            }

            var responseNonGet = await httpClient.SendAsync(requestMessageNonGet);

            if (!responseNonGet.IsSuccessStatusCode)
            {
                var errorContent = await responseNonGet.Content.ReadFromJsonAsync<dynamic>();
                return new ObjectResult(errorContent)
                { StatusCode = (int)responseNonGet.StatusCode };
            }

            if (responseNonGet.StatusCode == HttpStatusCode.NoContent)
                return new NoContentResult();

            var responseDataNonGet = await responseNonGet.Content.ReadFromJsonAsync<T>();

            logger.LogInformation("[INFO]: Status code: {sc}", responseNonGet.StatusCode);

            return responseNonGet.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(responseDataNonGet),
                HttpStatusCode.Created => new CreatedResult(fullUrl, responseDataNonGet),
                HttpStatusCode.BadRequest => new BadRequestResult(),
                HttpStatusCode.NotFound => new NotFoundResult(),
                HttpStatusCode.InternalServerError => new ObjectResult(
                    await responseNonGet.Content.ReadFromJsonAsync<dynamic>())
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                },
                _ => throw new ArgumentOutOfRangeException()
            };
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

    private static string? GetGroupFromEndpoint(string endpoint)
    {
        var segments = endpoint.TrimStart('/').Split('/');
        return segments.Length > 0 ? segments[0] : null;
    }

    private async Task InvalidateCacheGroupAsync(ServiceType serviceType, string group)
    {
        var setKey = GetCacheGroupKey(serviceType, group);
        var keys = await redisDb.SetMembersAsync(setKey);

        foreach (var key in keys)
        {
            await hybridCache.RemoveAsync(key.ToString());
            logger.LogInformation("[INFO]: Cache invalidated for key: {Key}", key);
        }

        await redisDb.KeyDeleteAsync(setKey);
    }
}
