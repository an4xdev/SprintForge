package org.student.microserviceapp.javaservice.dto.project;

import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import org.student.microserviceapp.javaservice.dto.company.CompanyDTO;
import org.student.microserviceapp.javaservice.models.Project;

import java.time.LocalDate;
import java.util.UUID;

@Getter
@Setter
@NoArgsConstructor
public class ProjectDTO {

    private UUID id;


    private String name;


    private LocalDate startDate;


    private LocalDate endDate;


    private CompanyDTO companyDto;

    public ProjectDTO(Project project){
        id = project.getId();
        name = project.getName();
        startDate = project.getStartDate();
        endDate = project.getEndDate();
        companyDto = new CompanyDTO(project.getCompany());
    }

}
