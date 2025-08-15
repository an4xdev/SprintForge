package org.student.microserviceapp.javaservice.models;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.annotations.OnDelete;
import org.hibernate.annotations.OnDeleteAction;

import java.time.LocalDate;
import java.util.UUID;

@Getter
@Setter
@Entity
@Table(name = "\"Projects\"", schema = "public", indexes = {
        @Index(name = "IX_Projects_CompanyId", columnList = "CompanyId")
})
public class Project {
    @Id
    @Column(name = "\"Id\"", nullable = false)
    private UUID id;

    @NotNull
    @Column(name = "\"Name\"", nullable = false, length = Integer.MAX_VALUE)
    private String name;

    @NotNull
    @Column(name = "\"StartDate\"", nullable = false)
    private LocalDate startDate;

    @NotNull
    @Column(name = "\"EndDate\"", nullable = false)
    private LocalDate endDate;

    @NotNull
    @ManyToOne(fetch = FetchType.LAZY, optional = false)
    @OnDelete(action = OnDeleteAction.CASCADE)
    @JoinColumn(name = "\"CompanyId\"", nullable = false)
    private Company company;

}