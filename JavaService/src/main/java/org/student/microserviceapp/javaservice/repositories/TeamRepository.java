package org.student.microserviceapp.javaservice.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.student.microserviceapp.javaservice.models.Team;

import java.util.List;
import java.util.UUID;

public interface TeamRepository extends JpaRepository<Team, UUID> {
    @Query("SELECT t FROM Team t JOIN FETCH t.manager WHERE t.manager.id = :managerId")
    List<Team> findTeamsByManager_Id(@Param("managerId") UUID managerId);
}
