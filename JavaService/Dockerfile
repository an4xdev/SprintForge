FROM maven:3.8.4-openjdk-17-slim AS build
WORKDIR /app

COPY pom.xml .

RUN mvn dependency:go-offline -B

COPY src ./src

RUN mvn clean package -DskipTests -B

FROM eclipse-temurin:17-jre-alpine AS final
WORKDIR /app

RUN apk add --no-cache curl

COPY --from=build /app/target/JavaService-0.0.1-SNAPSHOT.jar /app/javaservice.jar

HEALTHCHECK --interval=10s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/actuator/health || exit 1

EXPOSE 8080

ENTRYPOINT ["java", "-jar", "javaservice.jar"]