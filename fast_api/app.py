import json
from fastapi import FastAPI, Depends, Request, Response
from uuid import UUID
from fastapi.responses import JSONResponse
from sqlmodel import Session, desc, select, asc
from datetime import datetime, timezone
import uuid
import time
from fastapi.exceptions import RequestValidationError
from database import get_db, check_db_connection
from api_exception import ApiException
from api_response import ApiResponse
from models import Tasks, TaskHistories, TaskStatuses, Sprints, Users, \
    Teams, Projects, SprintCreate, SprintUpdate
from rabbit_mq_config import RabbitMQConfig
from audit_service import audit_service

app = FastAPI()

rabbitmq = RabbitMQConfig()
rabbitmq.connect()


def check_rabbitmq_connection():
    try:
        return rabbitmq.is_connected()
    except Exception:
        return False

@app.middleware("http")
async def log_requests(request: Request, call_next):
    print(f"\n{'='*50}")
    print(f"REQUEST DEBUG - {datetime.now()}")
    print(f"{'='*50}")
    print(f"Method: {request.method}")
    print(f"URL: {request.url}")
    print(f"Path: {request.url.path}")
    print(f"Query params: {dict(request.query_params)}")
    
    print(f"\nHEADERS:")
    for name, value in request.headers.items():
        print(f"  {name}: {value}")
    
    body = None
    if request.method in ["POST", "PUT", "PATCH"]:
        body = await request.body()
        print(f"\nRAW BODY ({len(body)} bytes):")
        print(f"Raw bytes: {body.decode('utf-8', errors='replace')}")
        
        if body:
            try:
                body_str = body.decode('utf-8')
                print(f"Decoded string: {repr(body_str)}")
                
                try:
                    json_data = json.loads(body_str)
                    print(f"Parsed JSON:")
                    print(json.dumps(json_data, indent=2, ensure_ascii=False))
                except json.JSONDecodeError as e:
                    print(f"JSON Parse Error: {e}")
            except UnicodeDecodeError as e:
                print(f"Unicode Decode Error: {e}")
    
    print(f"{'='*50}\n")
    
    response = await call_next(request)
    return response


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
def update_task_status(task_id: UUID, new_status: str, db: Session):
    task = db.exec(select(Tasks).where(Tasks.Id == task_id)).first()
    if not task:
        audit_service.log_action("UPDATE_FAILED", "Task", f"Failed to update task with ID {task_id}: task not found.")
        raise ApiException(status_code=404, message="Task not found")
    
    current_status = db.exec(
        select(TaskStatuses).where(TaskStatuses.Id == task.TaskStatusId)
    ).first()
    
    if not current_status:
        audit_service.log_action("UPDATE_FAILED", "Task", f"Failed to update task with ID {task_id}: current status not found.")
        raise ApiException(status_code=500, message="Current task status not found")
    
    task_history_prev = db.exec(
        select(TaskHistories)
        .where(TaskHistories.TaskId == task_id)
        .order_by(desc(TaskHistories.ChangeDate))
        .limit(1)
    ).first()
    
    old_status = task_history_prev.NewStatus if task_history_prev else current_status.Name
    
    task_history = TaskHistories(
        Id=uuid.uuid4(),
        TaskId=task_id,
        ChangeDate=datetime.now(timezone.utc),
        NewStatus=new_status,
        OldStatus=old_status
    )
    
    new_status_record = db.exec(
        select(TaskStatuses).where(TaskStatuses.Name == new_status)
    ).first()
    
    if not new_status_record:
        audit_service.log_action("UPDATE_FAILED", "Task", f"Failed to update task with ID {task_id}: new status not found.")
        raise ApiException(status_code=500, message=f"{new_status} status not found")
    
    task.TaskStatusId = new_status_record.Id
    db.add(task_history)
    db.commit()

    db.refresh(task)
    db.flush()
    
    time.sleep(0.1)
    
    try:
        message = {"action": f"task_{new_status.lower()}", "task_id": str(task_id)}
        rabbitmq.publish_message(message)
    except Exception as e:
        print(f"Failed to send RabbitMQ message: {e}")

    audit_service.log_action("UPDATE_SUCCESS", "Task", f"Task {task.Name} status changed from {old_status} to {new_status}")
    return {"status": f"Task {new_status.lower()}", "taskId": str(task_id)}


@app.put("/api/tasks/{task_id}/start")
def start_task(task_id: UUID, db: Session=Depends(get_db)):
    try:
        result = update_task_status(task_id, "Started", db)
        return ApiResponse(
            message="Task started",
            data=result
        ).model_dump()
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))


@app.put("/api/tasks/{task_id}/pause")
def pause_task(task_id: UUID, db: Session=Depends(get_db)):
    try:
        result = update_task_status(task_id, "Paused", db)
        return ApiResponse(
            message="Task paused",
            data=result
        ).model_dump()
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))


@app.put("/api/tasks/{task_id}/stop")
def stop_task(task_id: UUID, db: Session=Depends(get_db)):
    try:
        result = update_task_status(task_id, "Stopped", db)
        return ApiResponse(
            message="Task stopped",
            data=result
        ).model_dump()
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))


# Sprint management
@app.get("/api/sprints")
def get_sprints(db: Session=Depends(get_db)):
    statement = select(Sprints).order_by(asc(Sprints.StartDate), asc(Sprints.EndDate), asc(Sprints.Name))
    sprints = db.exec(statement).all()
    return ApiResponse(message="Sprints retrieved", data=sprints).model_dump()

# active or last active sprint
@app.get("/api/sprints/manager/{managerId}/dashboard")
def get_last_active_sprint(managerId: UUID, db: Session=Depends(get_db)):
    user = db.exec(select(Users).where(Users.Id == managerId)).first()
    if not user:
        raise ApiException(status_code=404, message="Manager not found")
    if user.Role != "manager":
        raise ApiException(status_code=403, message="User is not a manager")

    statement = select(Sprints).where(Sprints.ManagerId == managerId).order_by(desc(Sprints.EndDate))
    
    sprint = db.exec(statement).first()
    if not sprint:
        raise ApiException(status_code=404, message="No sprint found")

    active = True

    if sprint.EndDate and sprint.EndDate < datetime.now(timezone.utc).date():
        active = False

    sprint_data = {
        "id": str(sprint.Id),
        "name": sprint.Name,
        "active": active,
        "startDate": sprint.StartDate,
        "endDate": sprint.EndDate
    }
    return ApiResponse(message="Sprint data for dashboard retrieved", data=sprint_data).model_dump()

@app.get("/api/sprints/manager/{managerId}/last")
def get_last_sprint_by_manager(managerId: UUID, db: Session=Depends(get_db)):
    user = db.exec(select(Users).where(Users.Id == managerId)).first()
    if not user:
        raise ApiException(status_code=404, message="Manager not found")
    if user.Role != "manager":
        raise ApiException(status_code=403, message="User is not a manager")

    statement = select(Sprints).where(Sprints.ManagerId == managerId)
    sprint = db.exec(statement).first()
    if not sprint:
        raise ApiException(status_code=404, message="No sprint found")
    return ApiResponse(message="Last sprint retrieved", data=sprint.Id).model_dump()

@app.get("/api/sprints/manager/{managerId}")
def get_sprint_by_manager(managerId: UUID, db: Session=Depends(get_db)):
    user = db.exec(select(Users).where(Users.Id == managerId)).first()
    if not user:
        raise ApiException(status_code=404, message="Manager not found")
    if user.Role != "manager":
        raise ApiException(status_code=403, message="User is not a manager")

    statement = select(Sprints).where(Sprints.ManagerId == managerId).order_by(asc(Sprints.StartDate), asc(Sprints.EndDate), asc(Sprints.Name))
    sprints = db.exec(statement).all()
    return ApiResponse(message="Sprints retrieved", data=sprints).model_dump()


@app.post("/api/sprints")
def create_sprint(sprint: SprintCreate, db: Session=Depends(get_db)):
    try:
        user = db.exec(select(Users).where(Users.Id == sprint.ManagerId)).first()
        if not user:
            audit_service.log_action("CREATE_FAILED", "Sprint", f"Failed to create sprint {sprint.Name}: manager not found.")
            raise ApiException(status_code=404, message="Manager not found")
        if user.Role != "manager":
            audit_service.log_action("CREATE_FAILED", "Sprint", f"Failed to create sprint {sprint.Name}: user is not a manager.")
            raise ApiException(status_code=403, message="User is not a manager")
        team = db.exec(select(Teams).where(Teams.Id == sprint.TeamId)).first()
        if not team:
            audit_service.log_action("CREATE_FAILED", "Sprint", f"Failed to create sprint {sprint.Name}: team not found.")
            raise ApiException(status_code=404, message="Team not found")
        project = db.exec(select(Projects).where(Projects.Id == sprint.ProjectId)).first()
        if not project:
            audit_service.log_action("CREATE_FAILED", "Sprint", f"Failed to create sprint {sprint.Name}: project not found.")
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

        audit_service.log_action("CREATE_SUCCESS", "Sprint", f"Created new sprint: {sprint.Name}")

        return ApiResponse(message="Sprint created", data=new_sprint.Id).model_dump()
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
def update_sprint(sprint_id: UUID, sprint_update: SprintUpdate, db: Session=Depends(get_db)):
    try:
        statement = select(Sprints).where(Sprints.Id == sprint_id)
        db_sprint = db.exec(statement).first()
        
        if db_sprint is None:
            audit_service.log_action("UPDATE_FAILED", "Sprint", f"Failed to update sprint with ID {sprint_id}: sprint not found.")
            raise ApiException(status_code=404, message="Sprint not found")
        
        if sprint_update.ManagerId:
            user = db.exec(select(Users).where(Users.Id == sprint_update.ManagerId)).first()
            if not user:
                audit_service.log_action("UPDATE_FAILED", "Sprint", f"Failed to update sprint with ID {sprint_id}: manager not found.")
                raise ApiException(status_code=404, message="Manager not found")
            if user.Role != "manager":
                audit_service.log_action("UPDATE_FAILED", "Sprint", f"Failed to update sprint with ID {sprint_id}: user is not a manager.")
                raise ApiException(status_code=403, message="User is not a manager")
        
        if sprint_update.TeamId:
            team = db.exec(select(Teams).where(Teams.Id == sprint_update.TeamId)).first()
            if not team:
                audit_service.log_action("UPDATE_FAILED", "Sprint", f"Failed to update sprint with ID {sprint_id}: team not found.")
                raise ApiException(status_code=404, message="Team not found")
        
        if sprint_update.ProjectId:
            project = db.exec(select(Projects).where(Projects.Id == sprint_update.ProjectId)).first()
            if not project:
                audit_service.log_action("UPDATE_FAILED", "Sprint", f"Failed to update sprint with ID {sprint_id}: project not found.")
                raise ApiException(status_code=404, message="Project not found")
        
        update_data = sprint_update.model_dump(exclude_unset=True)
        for field, value in update_data.items():
            if value is not None:
                setattr(db_sprint, field, value)
        
        db.add(db_sprint)
        db.commit()
        db.refresh(db_sprint)
        audit_service.log_action("UPDATE_SUCCESS", "Sprint", f"Updated sprint with ID {sprint_id}.")
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
            audit_service.log_action("DELETE_FAILED", "Sprint", f"Failed to delete sprint with ID {sprint_id}: sprint not found.")
            raise ApiException(status_code=404, message="Sprint not found")
        
        db.delete(sprint)
        db.commit()
        audit_service.log_action("DELETE_SUCCESS", "Sprint", f"Deleted sprint with ID {sprint_id}.")
        return Response(status_code=204)
    except Exception as e:
        db.rollback()
        raise ApiException(status_code=500, message=str(e))


# Task time tracking
@app.get("/api/tasks/{task_id}/time")
def get_task_time(task_id: UUID, db: Session=Depends(get_db)):
    try:
        task = db.exec(select(Tasks).where(Tasks.Id == task_id)).first()
        if not task:
            raise ApiException(status_code=404, message="Task not found")
        
        total_seconds = calculate_task_total_time(task_id, db)
        
        current_status = db.exec(
            select(TaskStatuses).where(TaskStatuses.Id == task.TaskStatusId)
        ).first()
        
        last_history = db.exec(
            select(TaskHistories)
            .where(TaskHistories.TaskId == task_id)
            .order_by(desc(TaskHistories.ChangeDate))
            .limit(1)
        ).first()
        
        is_running = current_status and current_status.Name == "Started"
        current_session_start = None
        
        if is_running and last_history:
            current_session_start = last_history.ChangeDate
        
        return ApiResponse(
            message="Task time retrieved",
            data={
                "taskId": str(task_id),
                "totalSeconds": total_seconds,
                "isRunning": is_running,
                "currentStatus": current_status.Name if current_status else "Unknown",
                "currentSessionStart": current_session_start.isoformat() if current_session_start else None
            }
        ).model_dump()
        
    except Exception as e:
        raise ApiException(status_code=500, message=str(e))


@app.get("/api/tasks/developer/{developer_id}/times")
def get_developer_task_times(developer_id: UUID, db: Session=Depends(get_db)):
    try:
        tasks = db.exec(
            select(Tasks).where(Tasks.DeveloperId == developer_id)
        ).all()
        
        task_times = []
        for task in tasks:
            total_seconds = calculate_task_total_time(task.Id, db)
            
            current_status = db.exec(
                select(TaskStatuses).where(TaskStatuses.Id == task.TaskStatusId)
            ).first()
            
            last_history = db.exec(
                select(TaskHistories)
                .where(TaskHistories.TaskId == task.Id)
                .order_by(desc(TaskHistories.ChangeDate))
                .limit(1)
            ).first()
            
            is_running = current_status and current_status.Name == "Started"
            current_session_start = None
            
            if is_running and last_history:
                current_session_start = last_history.ChangeDate
            
            task_times.append({
                "taskId": str(task.Id),
                "taskName": task.Name,
                "totalSeconds": total_seconds,
                "isRunning": is_running,
                "currentStatus": current_status.Name if current_status else "Unknown",
                "currentSessionStart": current_session_start.isoformat() if current_session_start else None
            })
        
        return ApiResponse(
            message="Developer task times retrieved",
            data=task_times
        ).model_dump()
        
    except Exception as e:
        raise ApiException(status_code=500, message=str(e))


def calculate_task_total_time(task_id: UUID, db: Session) -> int:
    histories = db.exec(
        select(TaskHistories)
        .where(TaskHistories.TaskId == task_id)
        .order_by(asc(TaskHistories.ChangeDate))
    ).all()
    
    total_seconds = 0
    current_start = None
    
    for history in histories:
        if history.NewStatus == "Started":
            current_start = history.ChangeDate
        elif history.NewStatus in ["Paused", "Stopped"] and current_start:
            duration = history.ChangeDate - current_start
            total_seconds += int(duration.total_seconds())
            current_start = None
    
    if current_start:
        current_duration = datetime.now(timezone.utc) - current_start
        total_seconds += int(current_duration.total_seconds())
    
    return total_seconds

