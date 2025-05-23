package org.student.microserviceapp.javaservice.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.student.microserviceapp.javaservice.models.Company;

public interface CompanyRepository extends JpaRepository<Company, Integer> {
}
