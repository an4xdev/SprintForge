package org.student.microserviceapp.javaservice.services.company;

import org.springframework.stereotype.Service;
import org.student.microserviceapp.javaservice.models.Company;
import org.student.microserviceapp.javaservice.repositories.CompanyRepository;

import java.util.Optional;


@Service
public class CompanyService implements ICompanyService {
    private final CompanyRepository companyRepository;

    public CompanyService(CompanyRepository companyRepository) {
        this.companyRepository = companyRepository;
    }

    @Override
    public Optional<Company> getCompanyById(int id) {
        return companyRepository.findById(id);
    }
}
