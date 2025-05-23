package org.student.microserviceapp.javaservice.services.project;

import org.springframework.stereotype.Service;
import org.student.microserviceapp.javaservice.dto.project.CreateProjectDTO;
import org.student.microserviceapp.javaservice.dto.project.ProjectDTO;
import org.student.microserviceapp.javaservice.repositories.ProjectRepository;
import org.student.microserviceapp.javaservice.responses.Result;
import org.student.microserviceapp.javaservice.services.company.ICompanyService;

import java.util.List;
import java.util.UUID;

@Service
public class ProjectService implements IProjectService {
    private final ProjectRepository projectRepository;
    private final ICompanyService companyService;

    public ProjectService(ProjectRepository projectRepository, ICompanyService companyService) {
        this.projectRepository = projectRepository;
        this.companyService = companyService;
    }

    @Override
    public Result<ProjectDTO> getProjectById(UUID id) {
        var project = projectRepository.findById(id);
        return project
                .map(value -> Result.success(new ProjectDTO(value), "Project found"))
                .orElseGet(() -> Result.notFound("Project not found"));
    }

    @Override
    public Result<List<ProjectDTO>> getAllProjects() {
        var projects = projectRepository.findAll();
        if (projects.isEmpty()) {
            return Result.notFound("No projects found");
        }
        var projectDTOs = projects.stream()
                .map(ProjectDTO::new)
                .toList();
        return Result.success(projectDTOs, "Projects found");
    }

    @Override
    public Result<UUID> createProject(CreateProjectDTO createProjectDTO) {
        var company = companyService.getCompanyById(createProjectDTO.getCompanyId());
        if(company.isEmpty()) {
            return Result.notFound("Company not found");
        }
        var project = createProjectDTO.toProject();
        project.setId(UUID.randomUUID());
        project.setCompany(company.get());
        projectRepository.save(project);
        return Result.success(project.getId(), "Project created successfully");
    }

    @Override
    public Result<ProjectDTO> updateProject(UUID id, CreateProjectDTO createProjectDTO) {
        return null;
    }

    @Override
    public Result<Void> deleteProject(UUID id) {
        var project = projectRepository.findById(id);
        if(project.isEmpty()) {
            return Result.notFound("Project not found");
        }
        projectRepository.delete(project.get());
        return Result.noContent();
    }
}
