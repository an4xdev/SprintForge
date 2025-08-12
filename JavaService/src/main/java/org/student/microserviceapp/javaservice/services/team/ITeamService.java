package org.student.microserviceapp.javaservice.services.team;

import org.student.microserviceapp.javaservice.dto.team.CreateTeamDTO;
import org.student.microserviceapp.javaservice.dto.team.TeamDTO;
import org.student.microserviceapp.javaservice.responses.Result;

import java.util.List;
import java.util.UUID;

public interface ITeamService {
    Result<TeamDTO> getTeamById(UUID id);

    Result<List<TeamDTO>> getAllTeams();

    Result<UUID> createTeam(CreateTeamDTO createTeamDTO);

    Result<TeamDTO> updateTeam(UUID id, CreateTeamDTO createTeamDTO);

    Result<Void> deleteTeam(UUID id);

    Result<Long> getCount();

    Result<List<TeamDTO>> getTeamsByManagerId(UUID managerId);
}
