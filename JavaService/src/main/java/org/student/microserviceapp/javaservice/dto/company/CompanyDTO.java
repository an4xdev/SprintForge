package org.student.microserviceapp.javaservice.dto.company;

import lombok.Getter;
import org.student.microserviceapp.javaservice.models.Company;

@Getter
public class CompanyDTO {

    private final Integer id;

    private final String name;

    public CompanyDTO(Company company) {
        id = company.getId();
        name = company.getName();
    }

    public Company toCompany() {
        Company company = new Company();
        company.setId(id);
        company.setName(name);
        return company;
    }
}
