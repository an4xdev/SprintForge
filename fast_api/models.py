from datetime import date, datetime
import uuid
from sqlmodel import Field, SQLModel
from pydantic import field_validator, model_validator


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
    Role: str = Field()


class Companies(SQLModel, table=True):
    __tablename__ = 'Companies'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: int = Field(default=None, primary_key=True)
    Name: str = Field()


class Teams(SQLModel, table=True):
    __tablename__ = 'Teams'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(default=None, primary_key=True)
    Name: str = Field()
    ManagerId: uuid.UUID = Field(default="00000000-0000-0000-0000-000000000000", foreign_key="public.Users.Id")


class Projects(SQLModel, table=True):
    __tablename__ = 'Projects'
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
    CompanyId: int = Field(foreign_key="public.Companies.Id")


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
    ManagerId: uuid.UUID = Field(default="00000000-0000-0000-0000-000000000000", foreign_key="public.Users.Id")
    TeamId: uuid.UUID = Field(default="00000000-0000-0000-0000-000000000000", foreign_key="public.Teams.Id")
    ProjectId: uuid.UUID = Field(default="00000000-0000-0000-0000-000000000000", foreign_key="public.Projects.Id")


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
    TaskStatusId: int = Field(foreign_key="public.TaskStatuses.Id")
    DeveloperId: uuid.UUID = Field(foreign_key="public.Users.Id")


class TaskHistories(SQLModel, table=True):
    __tablename__ = 'TaskHistories'
    __table_args__ = (
        {
            "extend_existing": True,
            "schema": "public",
        }
    )

    Id: uuid.UUID = Field(default=None, primary_key=True)
    TaskId: uuid.UUID = Field(foreign_key="public.Tasks.Id")
    ChangeDate: date = Field()
    NewStatus: str = Field()
    OldStatus: str | None = Field(default=None)


class SprintCreate(SQLModel):
    Name: str
    StartDate: date
    EndDate: date
    ManagerId: uuid.UUID
    TeamId: uuid.UUID
    ProjectId: uuid.UUID
    
    @field_validator('StartDate', 'EndDate', mode='before')
    @classmethod
    def convert_datetime_to_date(cls, value):
        if isinstance(value, datetime):
            return value.date()
        return value
    
    @model_validator(mode='after')
    @classmethod
    def validate_dates(cls, model):
        if model.StartDate >= model.EndDate:
            raise ValueError('Start date must be before end date')
        return model
    
    @model_validator(mode='after')
    @classmethod
    def validateDatesNotInPast(cls, model):
        today = date.today()
        if model.StartDate < today or model.EndDate < today:
            raise ValueError('Start and end dates cannot be in the past')
        return model
