package org.student.microserviceapp.javaservice.responses;

import lombok.Getter;
import org.springframework.http.HttpStatus;

@Getter
public enum ResultStatus {
    OK(HttpStatus.OK),
    CREATED(HttpStatus.CREATED),
    NO_CONTENT(HttpStatus.NO_CONTENT),
    BAD_REQUEST(HttpStatus.BAD_REQUEST),
    NOT_FOUND(HttpStatus.NOT_FOUND),
    INTERNAL_ERROR(HttpStatus.INTERNAL_SERVER_ERROR);

    private final HttpStatus httpStatus;

    ResultStatus(HttpStatus httpStatus) {
        this.httpStatus = httpStatus;
    }

}
