package org.student.microserviceapp.javaservice.dto.project;

import jakarta.validation.constraints.FutureOrPresent;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import org.student.microserviceapp.javaservice.models.Project;
import org.student.microserviceapp.javaservice.validation.ValidDateRange;

import java.time.LocalDate;

@Getter
@ValidDateRange
public class CreateProjectDTO {

    @NotBlank(message = "Project name cannot be blank")
    @NotNull(message = "Project name is required")
    private String name;

    @NotNull(message = "Start date is required")
    @FutureOrPresent(message = "Start date must be today or in the future")
    private LocalDate startDate;

    @NotNull(message = "End date is required")
    @FutureOrPresent(message = "End date must be today or in the future")
    private LocalDate endDate;

    @NotNull(message = "Company ID is required")
    private Integer companyId;

    public Project toProject() {
        Project project = new Project();
        project.setName(name);
        project.setStartDate(startDate);
        project.setEndDate(endDate);
        return project;
    }
}
