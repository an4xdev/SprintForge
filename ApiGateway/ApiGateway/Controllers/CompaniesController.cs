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
    public async Task<ActionResult<ApiResponse<List<CompanyDto>>>> GetAllCompanies()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<CompanyDto>>>(HttpMethod.Get, "/companies",
            ServiceType.ExpressService);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> GetCompany(int id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<CompanyDto>>(HttpMethod.Get, $"/companies/{id}",
            ServiceType.ExpressService);
    }

    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<int>>> GetAllCompaniesCount()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<int>>(HttpMethod.Get, "/companies/count", ServiceType.ExpressService);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> CreateCompany(CompanyDto companyDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<CompanyDto>>(HttpMethod.Post, "/companies",
            ServiceType.ExpressService, body: companyDto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<CompanyDto>>> UpdateCompany(int id, CompanyDto companyDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<CompanyDto>>(HttpMethod.Put, $"/companies/{id}",
            ServiceType.ExpressService, body: companyDto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<object?>> DeleteCompany(int id)
    {
        return await sendRequestService.SendRequestAsync<object?>(HttpMethod.Delete, $"/companies/{id}",
            ServiceType.ExpressService);
    }
}