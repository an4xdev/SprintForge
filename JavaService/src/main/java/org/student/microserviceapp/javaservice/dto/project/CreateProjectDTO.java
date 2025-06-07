package org.student.microserviceapp.javaservice.dto.project;

import jakarta.validation.constraints.FutureOrPresent;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import org.student.microserviceapp.javaservice.models.Project;

import java.time.LocalDate;

@Getter
public class CreateProjectDTO {

    @NotBlank
    @NotNull
    private String name;

    @NotNull
    @FutureOrPresent
    private LocalDate startDate;

    @NotNull
    @FutureOrPresent
    private LocalDate endDate;

    @NotNull
    private Integer companyId;

    public Project toProject() {
        Project project = new Project();
        project.setName(name);
        project.setStartDate(startDate);
        project.setEndDate(endDate);
        return project;
    }
}
