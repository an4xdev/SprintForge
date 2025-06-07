package org.student.microserviceapp.javaservice.models;

// got from SharedObjects/AppDbContext/AppDbContext.cs, OnModelCreating

public class Roles {
    public static final String ADMIN = "admin";
    public static final String MANAGER = "manager";
    public static final String DEVELOPER = "developer";

    private Roles() {
    }
}
