using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "manager")]
public class TaskController(ISendRequestService requestService) : ControllerBase
{
    // TODO: implement
    // TODO: split by manager(managing tasks) and developer(start/stop)
}