package org.student.microserviceapp.javaservice.controllers;

import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.student.microserviceapp.javaservice.dto.project.CreateProjectDTO;
import org.student.microserviceapp.javaservice.dto.project.ProjectDTO;
import org.student.microserviceapp.javaservice.responses.ApiResponse;
import org.student.microserviceapp.javaservice.services.project.IProjectService;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/projects")
public class ProjectController {

    private final IProjectService projectService;

    public ProjectController(IProjectService projectService) {
        this.projectService = projectService;
    }

    @GetMapping
    public ResponseEntity<ApiResponse<List<ProjectDTO>>> getProjects() {
        var projects = projectService.getAllProjects();
        return projects.toResponseEntity();
    }

    @GetMapping("/{id}")
    public ResponseEntity<ApiResponse<ProjectDTO>> getProject(@PathVariable UUID id) {
        var project = projectService.getProjectById(id);
        return project.toResponseEntity();
    }

    @PostMapping
    public ResponseEntity<ApiResponse<UUID>> createProject(@Valid @RequestBody CreateProjectDTO createProjectDto) {
        var projectId = projectService.createProject(createProjectDto);
        return projectId.toResponseEntity();
    }

    @PutMapping("/{id}")
    public ResponseEntity<ApiResponse<ProjectDTO>> updateProject(@PathVariable UUID id, @RequestBody CreateProjectDTO createProjectDTO) {
        // TODO: implement updateProject
        var project = projectService.updateProject(id, createProjectDTO);
        return project.toResponseEntity();
    }

    @DeleteMapping("/{id}")
    public ResponseEntity<ApiResponse<Void>> deleteProject(@PathVariable UUID id) {
        var response = projectService.deleteProject(id);
        return response.toResponseEntity();
    }
}
