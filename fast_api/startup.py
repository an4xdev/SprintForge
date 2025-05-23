#!/usr/bin/env python3
import os
import sys
import uvicorn


def validate_env_variables():
    required_vars = [
        "DB_USERNAME",
        "DB_PASSWORD",
        "DB_HOST",
        "DB_PORT",
        "DB_DATABASE",
        "RABBITMQ_HOST",
        "RABBITMQ_PORT",
        "RABBITMQ_USER",
        "RABBITMQ_PASS"
    ]
    
    missing_vars = []
    for var in required_vars:
        if not os.getenv(var):
            missing_vars.append(var)
    
    if missing_vars:
        print(f"ERROR: Missing required environment variables: {', '.join(missing_vars)}")
        return False
    
    return True


if __name__ == "__main__":
    print("Checking environment variables...")
    
    if not validate_env_variables():
        print("Application cannot start due to missing environment variables.")
        sys.exit(1)
    
    print("All required environment variables are present.")
    print("Starting FastAPI application...")
    
    uvicorn.run("app:app", host="0.0.0.0", port=8000)
