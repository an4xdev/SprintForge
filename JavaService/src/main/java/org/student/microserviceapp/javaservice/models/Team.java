package org.student.microserviceapp.javaservice.models;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.annotations.ColumnDefault;
import org.hibernate.annotations.OnDelete;
import org.hibernate.annotations.OnDeleteAction;

import java.util.UUID;

@Getter
@Setter
@Entity
@Table(name = "\"Teams\"", schema = "public", indexes = {
        @Index(name = "IX_Teams_ManagerId", columnList = "ManagerId"),
        @Index(name = "IX_Teams_ProjectId", columnList = "ProjectId")
})
public class Team {
    @Id
    @Column(name = "\"Id\"", nullable = false)
    private UUID id;

    @NotNull
    @Column(name = "\"Name\"", nullable = false, length = Integer.MAX_VALUE)
    private String name;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @OnDelete(action = OnDeleteAction.CASCADE)
    @JoinColumn(name = "\"ManagerId\"", nullable = false)
    private User manager;

    @ManyToOne(fetch = FetchType.LAZY)
    @ColumnDefault("'00000000-0000-0000-0000-000000000000'")
    @JoinColumn(name = "\"ProjectId\"")
    private Project project;

}