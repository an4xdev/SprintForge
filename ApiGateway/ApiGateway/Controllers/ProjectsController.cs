using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedObjects.Responses;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ProjectsController(ISendRequestService sendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetAllProjects()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(HttpMethod.Get, "/projects",
            ServiceType.SpringService);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<ProjectDto>>(HttpMethod.Get, $"/projects/{id}",
            ServiceType.SpringService);
    }

    [HttpGet("active-count")]
    public async Task<ActionResult<ApiResponse<long>>> GetActiveProjectCount()
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<long>>(HttpMethod.Get, "/projects/active-count",
            ServiceType.SpringService);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateProject(CreateProjectDto projectDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<Guid>>(HttpMethod.Post, "/projects",
            ServiceType.SpringService, body: projectDto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, CreateProjectDto projectDto)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<ProjectDto>>(HttpMethod.Put, $"/projects/{id}",
            ServiceType.SpringService, body: projectDto);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteProject(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<object?>>(HttpMethod.Delete, $"/projects/{id}",
            ServiceType.SpringService);
    }

    [HttpGet("current/{id:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<ApiResponse<Guid>>> GetCurrentManagerProject(Guid id)
    {
        return await sendRequestService.SendRequestAsync<ApiResponse<Guid>>(HttpMethod.Get, $"/projects/current/{id}",
            ServiceType.SpringService);
    }

    [HttpGet("all-ext")]
    public async Task<ActionResult<ApiResponse<List<ProjectExtDto>>>> GetAllProjectsExt()
    {
        try
        {
            var projectsResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<ProjectDto>>>(
                HttpMethod.Get, "/projects", ServiceType.SpringService);

            var companiesResponse = await sendRequestService.SendRequestAsync<ApiResponse<List<CompanyDto>>>(
                HttpMethod.Get, "/companies", ServiceType.ExpressService);

            if (!Utils.IsSuccessResult(projectsResponse) || !Utils.IsSuccessResult(companiesResponse))
            {
                return Result<List<ProjectExtDto>>.InternalError("Failed to retrieve projects or companies data").ToActionResult();
            }

            var projectsData = ApiResponseExtensions.GetResultDataReference(projectsResponse);
            var companiesData = ApiResponseExtensions.GetResultDataReference(companiesResponse);

            if (projectsData == null)
            {
                return Result<List<ProjectExtDto>>.NotFound("No projects found").ToActionResult();
            }

            if (!projectsData.Any())
            {
                return Result<List<ProjectExtDto>>.Success([], "No projects available").ToActionResult();
            }

            var companiesDict = companiesData?.ToDictionary(c => c.Id, c => c) ?? new Dictionary<int, CompanyDto>();

            var projectsExt = projectsData.Select(project =>
            {
                var extendedCompany = companiesDict.TryGetValue(project.CompanyDto.Id, out var company) 
                    ? company 
                    : project.CompanyDto;

                return new ProjectExtDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Company = extendedCompany
                };
            }).ToList();

            return Result<List<ProjectExtDto>>.Success(projectsExt, "Projects retrieved with extended company data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<List<ProjectExtDto>>.InternalError("An unexpected error occurred while retrieving projects").ToActionResult();
        }
    }

    [HttpGet("{id:guid}-ext")]
    public async Task<ActionResult<ApiResponse<ProjectExtDto>>> GetProjectExt(Guid id)
    {
        try
        {
            var projectResponse = await sendRequestService.SendRequestAsync<ApiResponse<ProjectDto>>(
                HttpMethod.Get, $"/projects/{id}", ServiceType.SpringService);

            if (!Utils.IsSuccessResult(projectResponse))
            {
                return Result<ProjectExtDto>.InternalError("Failed to retrieve project data").ToActionResult();
            }

            var projectData = ApiResponseExtensions.GetResultDataReference(projectResponse);
            if (projectData == null)
            {
                return Result<ProjectExtDto>.NotFound("Project not found").ToActionResult();
            }

            var project = projectData;
            
            var companyResponse = await sendRequestService.SendRequestAsync<ApiResponse<CompanyDto>>(
                HttpMethod.Get, $"/companies/{project.CompanyDto.Id}", ServiceType.ExpressService);

            CompanyDto extendedCompany;
            
            if (!Utils.IsSuccessResult(companyResponse))
            {

                extendedCompany = project.CompanyDto;
            }
            else
            {
                var companyData = ApiResponseExtensions.GetResultDataReference(companyResponse);
                extendedCompany = companyData ?? project.CompanyDto;
            }

            var projectExt = new ProjectExtDto
            {
                Id = project.Id,
                Name = project.Name,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Company = extendedCompany
            };

            return Result<ProjectExtDto>.Success(projectExt, "Project retrieved with extended company data").ToActionResult();
        }
        catch (Exception)
        {
            return Result<ProjectExtDto>.InternalError("An unexpected error occurred while retrieving project").ToActionResult();
        }
    }
}