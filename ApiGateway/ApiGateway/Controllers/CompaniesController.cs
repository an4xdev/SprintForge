using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CompaniesController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CompanyDto>>>> GetAll()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<CompanyDto>>>(HttpMethod.Get, "/companies",
            ServiceType.ExpressService);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Get(int id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<CompanyDto>>(HttpMethod.Get, $"/companies/{id}",
            ServiceType.ExpressService);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Create(CompanyDto companyDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<CompanyDto>>(HttpMethod.Post, "/companies",
            ServiceType.ExpressService, body: companyDto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> Update(int id, CompanyDto companyDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<CompanyDto>>(HttpMethod.Put, $"/companies/{id}",
            ServiceType.ExpressService, body: companyDto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<object?>> Delete(int id)
    {
        return await sendRequestService.SendRequestAsync<object?>(HttpMethod.Delete, $"/companies/{id}",
            ServiceType.ExpressService);
    }
}