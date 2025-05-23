package org.student.microserviceapp.javaservice.services.team;

import org.springframework.stereotype.Service;
import org.student.microserviceapp.javaservice.dto.team.CreateTeamDTO;
import org.student.microserviceapp.javaservice.dto.team.TeamDTO;
import org.student.microserviceapp.javaservice.repositories.TeamRepository;
import org.student.microserviceapp.javaservice.responses.Result;
import org.student.microserviceapp.javaservice.services.user.IUserService;

import java.util.List;
import java.util.UUID;

@Service
public class TeamService implements ITeamService {
    private final TeamRepository teamRepository;
    private final IUserService userService;

    public TeamService(TeamRepository teamRepository, IUserService userService) {
        this.teamRepository = teamRepository;
        this.userService = userService;
    }

    @Override
    public Result<TeamDTO> getTeamById(UUID id) {
        var team = teamRepository.findById(id);
        if (team.isEmpty()) {
            return Result.notFound("Team not found");
        }
        var teamDTO = new TeamDTO(team.get());
        return Result.success(teamDTO, "Team found");
    }

    @Override
    public Result<List<TeamDTO>> getAllTeams() {
        var teams = teamRepository.findAll();
        if (teams.isEmpty()) {
            return Result.notFound("No teams found");
        }
        var teamDTOs = teams.stream()
                .map(TeamDTO::new)
                .toList();
        return Result.success(teamDTOs, "Teams found");
    }

    @Override
    public Result<UUID> createTeam(CreateTeamDTO createTeamDTO) {
        var user = userService.findById(createTeamDTO.getManagerId());
        if (user.isEmpty()) {
            return Result.notFound("User not found");
        }

        var team = createTeamDTO.toTeam();
        team.setId(UUID.randomUUID());
        team.setManager(user.get());
        teamRepository.save(team);
        return Result.success(team.getId(), "Team created successfully");
    }

    @Override
    public Result<TeamDTO> updateTeam(UUID id, CreateTeamDTO createTeamDTO) {
        return null;
    }

    @Override
    public Result<Void> deleteTeam(UUID id) {
        var team = teamRepository.findById(id);
        if (team.isEmpty()) {
            return Result.notFound("Team not found");
        }
        teamRepository.delete(team.get());
        return Result.noContent();
    }
}
