package org.student.microserviceapp.javaservice.models;

import jakarta.persistence.*;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.annotations.ColumnDefault;

import java.time.OffsetDateTime;
import java.util.UUID;

@Getter
@Setter
@Entity
@Table(name = "\"Users\"", schema = "public", indexes = {
        @Index(name = "IX_Users_Username", columnList = "Username", unique = true),
        @Index(name = "IX_Users_TeamId", columnList = "TeamId")
})
public class User {
    @Id
    @Column(name = "\"Id\"", nullable = false)
    private UUID id;

    @NotNull
    @Column(name = "\"Username\"", nullable = false, length = Integer.MAX_VALUE)
    private String username;

    @NotNull
    @Column(name = "\"PasswordHash\"", nullable = false, length = Integer.MAX_VALUE)
    private String passwordHash;

    @Size(max = 13)
    @NotNull
    @Column(name = "\"Role\"", nullable = false, length = 13)
    private String role;

    @Column(name = "\"RefreshToken\"", length = Integer.MAX_VALUE)
    private String refreshToken;

    @Column(name = "\"RefreshTokenExpiryTime\"")
    private OffsetDateTime refreshTokenExpiryTime;

    @Column(name = "\"Avatar\"", length = Integer.MAX_VALUE)
    private String avatar;

    @NotNull
    @ColumnDefault("''")
    @Column(name = "\"PasswordSalt\"", nullable = false, length = Integer.MAX_VALUE)
    private String passwordSalt;

    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "\"TeamId\"")
    private Team team;

    @NotNull
    @ColumnDefault("false")
    @Column(name = "\"NeedResetPassword\"", nullable = false)
    private Boolean needResetPassword = false;

}