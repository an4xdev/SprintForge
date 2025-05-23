from sqlmodel import create_engine, Session
from sqlalchemy.engine import Engine
import os
from urllib.parse import quote_plus
from typing import List, Optional, Generator
from sqlalchemy import text


def validate_env_variables() -> List[str]:
    required_vars = [
        "DB_USERNAME",
        "DB_PASSWORD",
        "DB_HOST",
        "DB_PORT",
        "DB_DATABASE"
    ]
    
    missing_vars = []
    for var in required_vars:
        if not os.getenv(var):
            missing_vars.append(var)
    
    return missing_vars


def get_database_config() -> dict[str, str | None]:
    missing_vars = validate_env_variables()
    if missing_vars:
        raise ValueError(f"Missing required environment variables: {', '.join(missing_vars)}")
    
    return {
        "user": os.getenv("DB_USERNAME"),
        "password": quote_plus(os.getenv("DB_PASSWORD") or ""),
        "host": os.getenv("DB_HOST"),
        "port": os.getenv("DB_PORT"),
        "database": os.getenv("DB_DATABASE")
    }


engine: Optional[Engine] = None
db_config = None

try:
    db_config = get_database_config()
    SQLALCHEMY_DATABASE_URL = (
        f"postgresql://{db_config['user']}:{db_config['password']}@"
        f"{db_config['host']}:{db_config['port']}/{db_config['database']}"
    )
    engine = create_engine(SQLALCHEMY_DATABASE_URL, echo=True)
except ValueError as e:
    print(f"Database configuration error: {e}")
    engine = None


def get_db() -> Generator[Session, None, None]:
    if engine is None:
        raise RuntimeError("Database engine not initialized due to missing environment variables")
    with Session(engine) as session:
        yield session


def check_db_connection() -> bool:
    if engine is None:
        return False
    
    try:
        with engine.connect() as conn:
            conn.execute(text("SELECT 1"))
        return True
    except Exception as e:
        print(f"Database connection error: {e}")
        return False
