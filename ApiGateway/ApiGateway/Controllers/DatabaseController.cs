using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "admin")]
public class DatabaseController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateDatabase()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<bool>>(HttpMethod.Get, "/database",
            ServiceType.DatabaseManager);
    }
}