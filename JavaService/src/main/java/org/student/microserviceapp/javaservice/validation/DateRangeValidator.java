package org.student.microserviceapp.javaservice.validation;

import jakarta.validation.ConstraintValidator;
import jakarta.validation.ConstraintValidatorContext;
import org.student.microserviceapp.javaservice.dto.project.CreateProjectDTO;

public class DateRangeValidator implements ConstraintValidator<ValidDateRange, CreateProjectDTO> {

    @Override
    public void initialize(ValidDateRange constraintAnnotation) {
    }

    @Override
    public boolean isValid(CreateProjectDTO dto, ConstraintValidatorContext context) {
        if (dto.getStartDate() == null || dto.getEndDate() == null) {
            return true;
        }

        return !dto.getEndDate().isBefore(dto.getStartDate());
    }
}
