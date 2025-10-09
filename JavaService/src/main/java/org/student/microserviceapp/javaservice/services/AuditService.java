package org.student.microserviceapp.javaservice.services;

import com.fasterxml.jackson.databind.ObjectMapper;
import lombok.extern.slf4j.Slf4j;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.time.ZoneOffset;
import java.util.HashMap;
import java.util.Map;

@Service
@Slf4j
public class AuditService {

    private final RabbitTemplate rabbitTemplate;
    private final ObjectMapper objectMapper;

    @Value("${spring.application.name:JavaService}")
    private String serviceName;

    public AuditService(RabbitTemplate rabbitTemplate, ObjectMapper objectMapper) {
        this.rabbitTemplate = rabbitTemplate;
        this.objectMapper = objectMapper;
    }

    public void logAction(String action, String entity, String description) {
        try {
            Map<String, Object> auditMessage = new HashMap<>();
            auditMessage.put("timestamp", LocalDateTime.now(ZoneOffset.UTC).toString());
            auditMessage.put("service", serviceName);
            auditMessage.put("action", action);
            auditMessage.put("entity", entity);
            auditMessage.put("description", description);

            rabbitTemplate.convertAndSend("audit_logs", "audit.javaservice", auditMessage);
            
            log.debug("Audit log sent: {} - {} - {}", action, entity, description);
        } catch (Exception ex) {
            log.error("Error publishing audit log: {}", ex.getMessage());
        }
    }
}