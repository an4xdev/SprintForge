package org.student.microserviceapp.javaservice.services.project;

import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.student.microserviceapp.javaservice.dto.project.CreateProjectDTO;
import org.student.microserviceapp.javaservice.dto.project.ProjectDTO;
import org.student.microserviceapp.javaservice.repositories.ProjectRepository;
import org.student.microserviceapp.javaservice.responses.Result;
import org.student.microserviceapp.javaservice.services.company.ICompanyService;

import java.time.LocalDate;
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
    @Transactional(readOnly = true)
    public Result<ProjectDTO> getProjectById(UUID id) {
        var project = projectRepository.findById(id);
        return project
                .map(value -> Result.success(new ProjectDTO(value), "Project found"))
                .orElseGet(() -> Result.notFound("Project not found"));
    }

    @Override
    @Transactional(readOnly = true)
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
    @Transactional
    public Result<UUID> createProject(CreateProjectDTO createProjectDTO) {
        var company = companyService.getCompanyById(createProjectDTO.getCompanyId());
        if (company.isEmpty()) {
            return Result.notFound("Company not found");
        }

        if (createProjectDTO.getStartDate().isAfter(createProjectDTO.getEndDate())) {
            return Result.badRequest("Start date cannot be after end date");
        }

        var project = createProjectDTO.toProject();
        project.setId(UUID.randomUUID());
        project.setCompany(company.get());
        projectRepository.save(project);
        return Result.success(project.getId(), "Project created successfully");
    }

    @Override
    @Transactional
    public Result<ProjectDTO> updateProject(UUID id, CreateProjectDTO createProjectDTO) {
        var project = projectRepository.findById(id);
        if (project.isEmpty()) {
            return Result.notFound("Project not found");
        }
        var existingProject = project.get();

        if (createProjectDTO.getName() != null && !createProjectDTO.getName().isBlank()) {
            existingProject.setName(createProjectDTO.getName());
        }

        if (createProjectDTO.getStartDate() != null && createProjectDTO.getEndDate() != null) {
            if (createProjectDTO.getStartDate().isAfter(createProjectDTO.getEndDate())) {
                return Result.badRequest("Start date cannot be after end date");
            }
            existingProject.setStartDate(createProjectDTO.getStartDate());
            existingProject.setEndDate(createProjectDTO.getEndDate());
        } else if (createProjectDTO.getStartDate() != null) {
            if (createProjectDTO.getStartDate().isAfter(existingProject.getEndDate())) {
                return Result.badRequest("Start date cannot be after existing end date");
            }
            existingProject.setStartDate(createProjectDTO.getStartDate());
        } else if (createProjectDTO.getEndDate() != null) {
            if (createProjectDTO.getEndDate().isBefore(existingProject.getStartDate())) {
                return Result.badRequest("End date cannot be before existing start date");
            }
            existingProject.setEndDate(createProjectDTO.getEndDate());
        }

        if (createProjectDTO.getCompanyId() != null) {
            var company = companyService.getCompanyById(createProjectDTO.getCompanyId());
            if (company.isEmpty()) {
                return Result.notFound("Company not found");
            }
            existingProject.setCompany(company.get());
        }

        projectRepository.save(existingProject);
        var projectDTO = new ProjectDTO(existingProject);
        return Result.success(projectDTO, "Project updated successfully");
    }

    @Override
    @Transactional
    public Result<Void> deleteProject(UUID id) {
        var project = projectRepository.findById(id);
        if (project.isEmpty()) {
            return Result.notFound("Project not found");
        }
        projectRepository.delete(project.get());
        return Result.noContent();
    }

    @Override
    public Result<Long> getActiveProjectCount() {
        var projectCount = projectRepository.countByStartDateBeforeAndEndDateAfter(
                LocalDate.now(), LocalDate.now());
        return Result.success(projectCount, "Project count retrieved successfully");
    }
}
