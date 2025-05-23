package org.student.microserviceapp.javaservice.dto.team;

import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;
import org.student.microserviceapp.javaservice.dto.user.UserDTO;
import org.student.microserviceapp.javaservice.models.Team;

import java.util.UUID;

@Getter
@Setter
@NoArgsConstructor
public class TeamDTO {
    private UUID id;

    private String name;

    private UserDTO Manager;

    public TeamDTO(Team team) {
        id = team.getId();
        name = team.getName();
        Manager = new UserDTO(team.getManager());
    }
}
