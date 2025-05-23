package org.student.microserviceapp.javaservice.models;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.annotations.OnDelete;
import org.hibernate.annotations.OnDeleteAction;

import java.util.UUID;

@Getter
@Setter
@Entity
@Table(name = "\"Teams\"", schema = "public", indexes = {
        @Index(name = "IX_Team_ManagerId", columnList = "ManagerId")
})
public class Team {
    @Id
    @Column(name = "\"Id\"", nullable = false)
    private UUID id;

    @Column(name = "\"Name\"", nullable = false, length = Integer.MAX_VALUE)
    private String name;

    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @OnDelete(action = OnDeleteAction.CASCADE)
    @JoinColumn(name = "\"ManagerId\"", nullable = false)
    private User manager;

}