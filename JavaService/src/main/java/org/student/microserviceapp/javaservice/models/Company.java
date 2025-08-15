package org.student.microserviceapp.javaservice.models;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
@Entity
@Table(name = "\"Companies\"", schema = "public")
public class Company {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "\"Id\"", nullable = false)
    private Integer id;

    @NotNull
    @Column(name = "\"Name\"", nullable = false, length = Integer.MAX_VALUE)
    private String name;

}