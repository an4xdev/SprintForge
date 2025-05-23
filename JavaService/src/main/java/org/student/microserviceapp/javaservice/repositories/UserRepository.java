package org.student.microserviceapp.javaservice.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.student.microserviceapp.javaservice.models.User;

import java.util.UUID;

public interface UserRepository extends JpaRepository<User, UUID> {
}
