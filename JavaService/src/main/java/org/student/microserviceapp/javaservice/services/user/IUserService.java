package org.student.microserviceapp.javaservice.services.user;

import org.student.microserviceapp.javaservice.models.User;

import java.util.Optional;
import java.util.UUID;

public interface IUserService {
    public Optional<User> findById(UUID id);
}
