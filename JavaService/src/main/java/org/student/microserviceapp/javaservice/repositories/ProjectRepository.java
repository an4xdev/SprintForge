package org.student.microserviceapp.javaservice.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.student.microserviceapp.javaservice.models.Project;

import java.time.LocalDate;
import java.util.Optional;
import java.util.UUID;

public interface ProjectRepository extends JpaRepository<Project, UUID> {

    Long countByStartDateBeforeAndEndDateAfter(LocalDate now, LocalDate now1);

    Optional<Project> findByName(String name);
}
