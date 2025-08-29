package org.student.microserviceapp.javaservice.services.project;

import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import org.student.microserviceapp.javaservice.dto.project.CreateProjectDTO;
import org.student.microserviceapp.javaservice.dto.project.ProjectDTO;
import org.student.microserviceapp.javaservice.models.Project;
import org.student.microserviceapp.javaservice.repositories.ProjectRepository;
import org.student.microserviceapp.javaservice.repositories.TeamRepository;
import org.student.microserviceapp.javaservice.responses.Result;
import org.student.microserviceapp.javaservice.services.company.ICompanyService;

import java.time.LocalDate;
import java.util.*;

@Service
public class ProjectService implements IProjectService {
    private final ProjectRepository projectRepository;
    private final ICompanyService companyService;
    private final TeamRepository teamRepository;

    public ProjectService(ProjectRepository projectRepository, ICompanyService companyService,
                          TeamRepository teamRepository) {
        this.projectRepository = projectRepository;
        this.companyService = companyService;
        this.teamRepository = teamRepository;
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
        var projects = projectRepository.findAll()
                .stream()
                .sorted(Comparator.comparing(Project::getStartDate))
                .toList();
        if (projects.isEmpty()) {
            return Result.success(new ArrayList<>(0), "No projects found");
        }
        var projectDTOs = projects.stream()
                .map(ProjectDTO::new)
                .toList();
        return Result.success(projectDTOs, "Projects found");
    }

    @Override
    @Transactional
    public Result<UUID> createProject(CreateProjectDTO createProjectDTO) {

        if (createProjectDTO.getStartDate().isAfter(createProjectDTO.getEndDate())) {
            return Result.badRequest("Start date cannot be after end date");
        }

        var project = createProjectDTO.toProject();
        project.setId(UUID.randomUUID());
        if (createProjectDTO.getCompanyId() != null) {
            var company = companyService.getCompanyById(createProjectDTO.getCompanyId());
            if (company.isEmpty()) {
                return Result.badRequest("Company not found");
            }
            project.setCompany(company.get());
        } else {
            var defaultCompany = companyService.getDefaultCompany();
            if (defaultCompany.isEmpty()) {
                return Result.internalError("No company provided and could not find default company.");
            }
            project.setCompany(defaultCompany.get());
        }
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

        if (Objects.equals(existingProject.getName(), "Default")) {
            return Result.badRequest("Default project cannot be modified");
        }

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
                return Result.badRequest("Company not found");
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
        if (Objects.equals(project.get().getName(), "Default")) {
            return Result.badRequest("Default project cannot be deleted");
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

    @Override
    public Result<UUID> getCurrentProjectByManagerId(UUID managerId) {
        var teamOptional = teamRepository.findTeamByManager_Id(managerId);
        if (teamOptional.isEmpty()) {
            return Result.notFound("Team not found for the given manager ID");
        }
        var team = teamOptional.get();
        return Result.success(team.getProject().getId(), "Current project ID retrieved successfully");
    }
}
