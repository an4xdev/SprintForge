from fastapi import FastAPI, Depends, Request, Response
from uuid import UUID
from fastapi.responses import JSONResponse
from sqlmodel import Session, desc, select
from datetime import datetime
import uuid
from fastapi.exceptions import RequestValidationError
from database import get_db, check_db_connection
from api_exception import ApiException
from api_response import ApiResponse
from models import Tasks, TaskHistories, TaskStatuses, Sprints, Users, \
    Teams, Projects, SprintCreate
from rabbit_mq_config import RabbitMQConfig

app = FastAPI()

rabbitmq = RabbitMQConfig()
rabbitmq.connect()


def check_rabbitmq_connection():
    try:
        return rabbitmq.is_connected()
    except Exception:
        return False


@app.get("/health")
def health_check():
    db_ok = check_db_connection()
    rabbitmq_ok = check_rabbitmq_connection()
    
    if not db_ok or not rabbitmq_ok:
        return JSONResponse(
            status_code=503,
            content={
                "status": "unhealthy",
                "services": {
                    "database": "connected" if db_ok else "disconnected",
                    "rabbitmq": "connected" if rabbitmq_ok else "disconnected"
                }
            }
        )
    
    return {
        "status": "healthy",
        "services": {
            "database": "connected",
            "rabbitmq": "connected"
        }
    }


@app.exception_handler(ApiException)
async def api_exception_handler(_: Request, exc: ApiException):
    return JSONResponse(
        status_code=exc.status_code,
        content=ApiResponse(message=exc.message).model_dump()
    )


@app.exception_handler(RequestValidationError)
async def validation_exception_handler(_: Request, exc: RequestValidationError):
    error_details = []
    for error in exc.errors():
        error_details.append({
            "msg": error.get("msg", ""),
        })
    
    return JSONResponse(
        status_code=422,
        content=ApiResponse(
            message="Validation error",
            data={"errors": error_details}
        ).model_dump()
    )


# Task management
@app.put("/api/tasks/{task_id}/start")
def start_task(task_id: UUID, db: Session=Depends(get_db)):
    try:
        task = db.exec(select(Tasks).where(Tasks.Id == task_id)).first()
        if not task:
            raise ApiException(status_code=404, message="Task not found")
        
        task_history_prev = db.exec(
            select(TaskHistories)
            .where(TaskHistories.TaskId == task_id)
            .order_by(desc(TaskHistories.ChangeDate))
            .limit(1)
        ).first()
        
        if not task_history_prev:
            raise ApiException(status_code=404, message="Task history not found")
        
        task_history = TaskHistories(
            Id=UUID(),
            TaskId=task_id,
            ChangeDate=datetime.now(),
            NewStatus="In Progress",
            OldStatus=task_history_prev.NewStatus
        )
        
        in_progress_status = db.exec(
            select(TaskStatuses).where(TaskStatuses.Name == "In Progress")
        ).first()
        
        if not in_progress_status:
            raise ApiException(status_code=500, message="In Progress status not found")
        
        task.TaskStatusId = in_progress_status.Id
        db.add(task_history)
        db.commit()
        
        try:
            message = {"action": "task_started", "task_id": str(task_id)}
            rabbitmq.publish_message(message)
        except Exception as e:
            print(f"Failed to send RabbitMQ message: {e}")
        
        return ApiResponse(
            message="Task started",
            data={"status": "Task started", "task_id": str(task_id)}
        ).model_dump()
        
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))


@app.put("/api/tasks/{task_id}/pause")
def pause_task(task_id: UUID, db: Session=Depends(get_db)):
    try:
        task = db.exec(select(Tasks).where(Tasks.Id == task_id)).first()
        if not task:
            raise ApiException(status_code=404, message="Task not found")
        
        task_history_prev = db.exec(
            select(TaskHistories)
            .where(TaskHistories.TaskId == task_id)
            .order_by(desc(TaskHistories.ChangeDate))
            .limit(1)
        ).first()
        
        if not task_history_prev:
            raise ApiException(status_code=404, message="Task history not found")
        
        task_history = TaskHistories(
            Id=UUID(),
            TaskId=task_id,
            ChangeDate=datetime.now(),
            NewStatus="Paused",
            OldStatus=task_history_prev.NewStatus
        )
        
        paused_status = db.exec(
            select(TaskStatuses).where(TaskStatuses.Name == "Paused")
        ).first()
        
        if not paused_status:
            raise ApiException(status_code=500, message="Paused status not found")
        
        task.TaskStatusId = paused_status.Id
        db.add(task_history)
        db.commit()
        
        try:
            message = {"action": "task_paused", "task_id": str(task_id)}
            rabbitmq.publish_message(message)
        except Exception as e:
            print(f"Failed to send RabbitMQ message: {e}")
        
        return ApiResponse(
            message="Task paused",
            data={"status": "Task paused", "task_id": str(task_id)}
        ).model_dump()
        
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))

    
# Sprint management
@app.get("/api/sprints/")
def get_sprints(db: Session=Depends(get_db)):
    statement = select(Sprints).order_by(desc(Sprints.StartDate))
    sprints = db.exec(statement).all()
    return ApiResponse(message="Sprints retrieved", data=sprints).model_dump()


@app.post("/api/sprints/")
def create_sprint(sprint: SprintCreate, db: Session=Depends(get_db)):
    try:
        user = db.exec(select(Users).where(Users.Id == sprint.ManagerId)).first()
        if not user:
            raise ApiException(status_code=404, message="Manager not found")
        if user.Role != "manager":
            raise ApiException(status_code=403, message="User is not a manager")
        team = db.exec(select(Teams).where(Teams.Id == sprint.TeamId)).first()
        if not team:
            raise ApiException(status_code=404, message="Team not found")
        project = db.exec(select(Projects).where(Projects.Id == sprint.ProjectId)).first()
        if not project:
            raise ApiException(status_code=404, message="Project not found")
        new_sprint = Sprints(
            Id=uuid.uuid4(),
            Name=sprint.Name,
            StartDate=sprint.StartDate,
            EndDate=sprint.EndDate,
            ManagerId=sprint.ManagerId,
            TeamId=sprint.TeamId,
            ProjectId=sprint.ProjectId
        )
        db.add(new_sprint)
        db.commit()
        db.refresh(new_sprint)
        return ApiResponse(message="Sprint created", data=new_sprint).model_dump()
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))

    
@app.get("/api/sprints/{sprint_id}")
def get_sprint(sprint_id: UUID, db: Session=Depends(get_db)):
    statement = select(Sprints).where(Sprints.Id == sprint_id)
    sprint = db.exec(statement).first()
    if not sprint:
        raise ApiException(status_code=404, message="Sprint not found")
    return ApiResponse(message="Sprint found", data=sprint).model_dump()


@app.put("/api/sprints/{sprint_id}")
def update_sprint(sprint_id: UUID, sprint_update: Sprints, db: Session=Depends(get_db)):
    try:
        statement = select(Sprints).where(Sprints.Id == sprint_id)
        db_sprint = db.exec(statement).first()
        
        if db_sprint is None:
            raise ApiException(status_code=404, message="Sprint not found")
        
        for field, value in sprint_update.model_dump(exclude_unset=True).items():
            if value is not None:
                setattr(db_sprint, field, value)
        
        db.add(db_sprint)
        db.commit()
        db.refresh(db_sprint)
        return ApiResponse(message="Sprint updated", data=db_sprint).model_dump()
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))


@app.delete("/api/sprints/{sprint_id}")
def delete_sprint(sprint_id: UUID, db: Session=Depends(get_db)):
    try:
        statement = select(Sprints).where(Sprints.Id == sprint_id)
        sprint = db.exec(statement).first()
        
        if sprint is None:
            raise ApiException(status_code=404, message="Sprint not found")
        
        db.delete(sprint)
        db.commit()
        return Response(status_code=204)
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))

