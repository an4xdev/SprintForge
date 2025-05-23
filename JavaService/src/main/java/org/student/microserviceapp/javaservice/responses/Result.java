package org.student.microserviceapp.javaservice.responses;

import lombok.Getter;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;

@Getter
public class Result<T> {
    private T data;
    private String message;
    private ResultStatus status;

    private Result(T data, String message, ResultStatus status) {
        this.data = data;
        this.message = message;
        this.status = status;
    }

    public static <T> Result<T> success(T data, String message) {
        return new Result<>(data, message, ResultStatus.OK);
    }

    public static <T> Result<T> created(T data, String message) {
        return new Result<>(data, message, ResultStatus.CREATED);
    }

    public static <T> Result<T> noContent() {
        return new Result<>(null, null, ResultStatus.NO_CONTENT);
    }

    public static <T> Result<T> badRequest(String message) {
        return new Result<>(null, message, ResultStatus.BAD_REQUEST);
    }

    public static <T> Result<T> notFound(String message) {
        return new Result<>(null, message, ResultStatus.NOT_FOUND);
    }

    public static <T> Result<T> internalError(String message) {
        return new Result<>(null, message, ResultStatus.INTERNAL_ERROR);
    }

    public ResponseEntity<ApiResponse<T>> toResponseEntity() {
        HttpStatus httpStatus = status.getHttpStatus();
        ApiResponse<T> response = new ApiResponse<>(message, data);
        return new ResponseEntity<>(response, httpStatus);
    }
}
