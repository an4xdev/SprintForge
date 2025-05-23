package org.student.microserviceapp.javaservice.dto.user;

import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import org.student.microserviceapp.javaservice.models.User;

import java.util.UUID;
@Getter
@Setter
@NoArgsConstructor
public class UserDTO {
    public UUID id;
    public String username;

    public UserDTO(User user) {
        this.id = user.getId();
        this.username = user.getUsername();
    }
}
