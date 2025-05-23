using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "admin")]
public class TaskTypeController(ISendRequestService requestService) : ControllerBase
{
    // TODO: implement
}