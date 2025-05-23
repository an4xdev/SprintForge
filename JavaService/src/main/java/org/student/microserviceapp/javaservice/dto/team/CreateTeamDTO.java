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

    @NotNull
    public UUID managerId;

    @NotNull
    @NotBlank
    public String name;

    public Team toTeam() {
        Team team = new Team();
        team.setName(name);
        return team;
    }
}
