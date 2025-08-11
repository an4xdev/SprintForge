using System.Text.Json;
using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "admin")]
public class AdminDashboardController(ISendRequestService sendRequestService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetAdminDashboardInfo()
    {
        try
        {
            var result = new AdminDashboardDto();

            var companiesResult = await sendRequestService.SendRequestAsync<ApiResponse<int>>(
                HttpMethod.Get, "/companies/count", ServiceType.ExpressService);

            var usersResult = await sendRequestService.SendRequestAsync<ApiResponse<int>>(
                HttpMethod.Get, "/users/count", ServiceType.AuthService);

            var projectsResult = await sendRequestService.SendRequestAsync<ApiResponse<long>>(
                HttpMethod.Get, "/projects/active-count", ServiceType.SpringService);

            var teamsResult = await sendRequestService.SendRequestAsync<ApiResponse<long>>(
                HttpMethod.Get, "/teams/count", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(companiesResult) || !Utils.IsSuccessResult(usersResult) ||
                !Utils.IsSuccessResult(projectsResult) || !Utils.IsSuccessResult(teamsResult))
            {
                return StatusCode(500, new ApiResponse<AdminDashboardDto>
                {
                    Message = "Some unexpected error occurred",
                    Data = null
                });
            }

            var companiesData = ApiResponseExtensions.GetResultData(companiesResult);
            var usersData = ApiResponseExtensions.GetResultData(usersResult);
            var projectsData = ApiResponseExtensions.GetResultData(projectsResult);
            var teamsData = ApiResponseExtensions.GetResultData(teamsResult);

            if (!companiesData.HasValue || !usersData.HasValue ||
                !projectsData.HasValue || !teamsData.HasValue)
            {
                return StatusCode(500, new ApiResponse<AdminDashboardDto>
                {
                    Message = "Some unexpected error occurred",
                    Data = null
                });
            }

            result.CompaniesCount = companiesData.Value;
            result.UsersCount = usersData.Value;
            result.ProjectsCount = projectsData.Value;
            result.TeamsCount = teamsData.Value;

            return Ok(new ApiResponse<AdminDashboardDto>
            {
                Message = "Data to admin dashboard has been successfully retrieved",
                Data = result
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse<AdminDashboardDto>
            {
                Message = "Some unexpected error occurred",
                Data = null
            });
        }
    }
}