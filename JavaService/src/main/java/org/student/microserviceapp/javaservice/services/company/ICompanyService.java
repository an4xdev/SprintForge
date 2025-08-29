package org.student.microserviceapp.javaservice.services.company;

import org.student.microserviceapp.javaservice.models.Company;

import java.util.Optional;

public interface ICompanyService {
    Optional<Company> getCompanyById(int id);
    Optional<Company> getDefaultCompany();
}
