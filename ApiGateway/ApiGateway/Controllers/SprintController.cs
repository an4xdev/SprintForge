using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "manager")]
public class SprintController(ISendRequestService requestService) : ControllerBase
{
    // TODO: implement
}