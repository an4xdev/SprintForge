package org.student.microserviceapp.javaservice.dto.team;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.Setter;
import org.student.microserviceapp.javaservice.models.Team;

import java.util.UUID;

@Getter
@Setter
public class CreateTeamDTO {

    @NotNull(message = "Manager ID is required")
    public UUID managerId;

    @NotNull(message = "Team name is required")
    @NotBlank(message = "Team name cannot be blank")
    public String name;

    public Team toTeam() {
        Team team = new Team();
        team.setName(name);
        return team;
    }
}
