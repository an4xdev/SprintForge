from datetime import date
import uuid
from sqlmodel import Field, SQLModel


class Sprints(SQLModel, table=True):
    __tablename__ = 'Sprints'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(default=None, primary_key=True)
    Name: str = Field()
    StartDate: date = Field()
    EndDate: date = Field()
    ManagerId: str = Field(default="00000000-0000-0000-0000-000000000000",foreign_key="Users.Id")
    TeamId: str = Field(default="00000000-0000-0000-0000-000000000000", foreign_key="Teams.Id")
    ProjectId: str = Field(default="00000000-0000-0000-0000-000000000000", foreign_key="Projects.Id")

class TaskStatuses(SQLModel, table=True):
    __tablename__ = 'TaskStatuses'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: int = Field(default=None, primary_key=True)
    Name: str = Field()

class Users(SQLModel, table=True):
    __tablename__ = 'Users'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(primary_key=True)
    Username: str = Field()

class Tasks(SQLModel, table=True):
    __tablename__ = 'Tasks'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(default=None, primary_key=True)
    Name: str = Field()
    TaskStatusId: int = Field(foreign_key="TaskStatuses.Id")
    DeveloperId: str = Field(foreign_key="Users.Id")

class TaskHistories(SQLModel, table=True):
    __tablename__ = 'TaskHistories'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(default=None, primary_key=True)
    TaskId: str = Field(foreign_key="Tasks.Id")
    ChangeDate: date = Field()
    NewStatus: str = Field()
    OldStatus: str | None = Field(default=None)

class Projects(SQLModel, table=True):
    __tablename__ = 'Projects'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(default=None, primary_key=True)

class Teams(SQLModel, table=True):
    __tablename__ = 'Teams'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(default=None, primary_key=True)