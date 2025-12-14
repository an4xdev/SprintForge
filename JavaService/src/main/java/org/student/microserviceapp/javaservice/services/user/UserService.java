package org.student.microserviceapp.javaservice.services.user;

import org.springframework.stereotype.Service;
import org.student.microserviceapp.javaservice.models.User;
import org.student.microserviceapp.javaservice.repositories.UserRepository;

import java.util.Optional;
import java.util.UUID;

@Service
public class UserService implements IUserService {

    private final UserRepository userRepository;

    public UserService(UserRepository userRepository) {
        this.userRepository = userRepository;
    }

    @Override
    public Optional<User> findById(UUID id) {
        return userRepository.findById(id);
    }

    @Override
    public User save(User user) {
        return userRepository.save(user);
    }
}
