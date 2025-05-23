package org.student.microserviceapp.javaservice.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.student.microserviceapp.javaservice.models.Project;

import java.util.UUID;

public interface ProjectRepository extends JpaRepository<Project, UUID> {
}
