package org.student.microserviceapp.javaservice.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.student.microserviceapp.javaservice.models.Team;

import java.util.UUID;

public interface TeamRepository extends JpaRepository<Team, UUID> {
}
