CREATE OR REPLACE FUNCTION GetManagerTasksWithDetails(
    p_manager_id UUID
)
    RETURNS TABLE
            (
                task_id        UUID,
                task_name      TEXT,
                total_duration INTERVAL,
                is_started     BOOLEAN,
                is_paused      BOOLEAN,
                is_stopped     BOOLEAN,
                current_status TEXT,
                developer_name TEXT,
                start_time     TIMESTAMP,
                update_time    TIMESTAMP,
                sprint_id      UUID,
                team_id        UUID
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    task_rec             RECORD;
    history_rec          RECORD;
    current_duration     INTERVAL;
    task_total_duration  INTERVAL;
    task_start_time      TIMESTAMP;
    is_currently_started BOOLEAN;
    last_status          TEXT;
    last_update_time     TIMESTAMP;
    first_start_time     TIMESTAMP;
BEGIN
    FOR task_rec IN
        SELECT t."Id",
               t."Name",
               t."SprintId",
               s."TeamId",
               COALESCE(u."Username", 'Unassigned') as developer_name,
               ts."Name"                            as initial_status
        FROM "Tasks" t
                 INNER JOIN "Sprints" s ON t."SprintId" = s."Id"
                 INNER JOIN "Teams" tm ON tm."Id" = s."TeamId"
                 INNER JOIN "TaskStatuses" ts ON t."TaskStatusId" = ts."Id"
                 LEFT JOIN "Users" u ON t."DeveloperId" = u."Id"
        WHERE s."ManagerId" = p_manager_id
          AND t."TaskStatusId" NOT IN (1, 2, 6) -- Exclude Created, Assigned, Completed
        ORDER BY t."Name"
        LOOP
            task_total_duration := INTERVAL '0';
            task_start_time := NULL;
            is_currently_started := FALSE;
            last_status := task_rec.initial_status;
            last_update_time := NOW();
            first_start_time := NULL;

            -- duration from task history
            FOR history_rec IN
                SELECT th."NewStatus", th."ChangeDate"
                FROM "TaskHistories" th
                WHERE th."TaskId" = task_rec."Id"
                ORDER BY th."ChangeDate"
                LOOP
                    last_status := history_rec."NewStatus";
                    last_update_time := history_rec."ChangeDate";

                    IF history_rec."NewStatus" = 'Started' THEN
                        task_start_time := history_rec."ChangeDate";
                        is_currently_started := TRUE;

                        -- first start time
                        IF first_start_time IS NULL THEN
                            first_start_time := history_rec."ChangeDate";
                        END IF;

                    ELSIF history_rec."NewStatus" IN ('Stopped', 'Paused') AND is_currently_started THEN
                        IF task_start_time IS NOT NULL THEN
                            current_duration := history_rec."ChangeDate" - task_start_time;
                            task_total_duration := task_total_duration + current_duration;
                        END IF;
                        is_currently_started := FALSE;
                        task_start_time := NULL;
                    END IF;
                END LOOP;

            -- current session duration if task is running
            IF is_currently_started AND task_start_time IS NOT NULL THEN
                current_duration := NOW() - task_start_time;
                task_total_duration := task_total_duration + current_duration;
            END IF;

            task_id := task_rec."Id";
            task_name := task_rec."Name";
            total_duration := task_total_duration;
            current_status := COALESCE(last_status, 'Created');
            is_started := (last_status = 'Started');
            is_paused := (last_status = 'Paused');
            is_stopped := (last_status = 'Stopped');
            developer_name := task_rec.developer_name;
            start_time := first_start_time;
            update_time := last_update_time;
            sprint_id := task_rec."SprintId"::UUID;
            team_id := task_rec."TeamId"::UUID;

            RETURN NEXT;
        END LOOP;

    RETURN;
END;
$$;

CREATE OR REPLACE FUNCTION GetSingleTaskDetails(
    p_task_id UUID
)
    RETURNS TABLE
            (
                task_id        UUID,
                task_name      TEXT,
                total_duration INTERVAL,
                is_started     BOOLEAN,
                is_paused      BOOLEAN,
                is_stopped     BOOLEAN,
                current_status TEXT,
                developer_name TEXT,
                start_time     TIMESTAMP,
                update_time    TIMESTAMP,
                sprint_id      UUID,
                team_id        UUID,
                manager_id     UUID
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    history_rec          RECORD;
    current_duration     INTERVAL;
    task_total_duration  INTERVAL;
    task_start_time      TIMESTAMP;
    is_currently_started BOOLEAN;
    last_status          TEXT;
    last_update_time     TIMESTAMP;
    first_start_time     TIMESTAMP;
    task_data            RECORD;
BEGIN
    SELECT t."Id",
           t."Name",
           t."SprintId",
           s."TeamId",
           s."ManagerId",
           COALESCE(u."Username", 'Unassigned') as developer_name,
           ts."Name"                            as initial_status
    INTO task_data
    FROM "Tasks" t
             INNER JOIN "Sprints" s ON t."SprintId" = s."Id"
             INNER JOIN "Teams" tm ON tm."Id" = s."TeamId"
             INNER JOIN "TaskStatuses" ts ON t."TaskStatusId" = ts."Id"
             LEFT JOIN "Users" u ON t."DeveloperId" = u."Id"
    WHERE t."Id" = p_task_id;

    IF NOT FOUND THEN
        RETURN;
    END IF;

    task_total_duration := INTERVAL '0';
    task_start_time := NULL;
    is_currently_started := FALSE;
    last_status := task_data.initial_status;
    last_update_time := NOW();
    first_start_time := NULL;

    FOR history_rec IN
        SELECT th."NewStatus", th."ChangeDate"
        FROM "TaskHistories" th
        WHERE th."TaskId" = p_task_id
        ORDER BY th."ChangeDate"
        LOOP
            last_status := history_rec."NewStatus";
            last_update_time := history_rec."ChangeDate";

            IF history_rec."NewStatus" = 'Started' THEN
                task_start_time := history_rec."ChangeDate";
                is_currently_started := TRUE;

                IF first_start_time IS NULL THEN
                    first_start_time := history_rec."ChangeDate";
                END IF;

            ELSIF history_rec."NewStatus" IN ('Stopped', 'Paused') AND is_currently_started THEN
                IF task_start_time IS NOT NULL THEN
                    current_duration := history_rec."ChangeDate" - task_start_time;
                    task_total_duration := task_total_duration + current_duration;
                END IF;
                is_currently_started := FALSE;
                task_start_time := NULL;
            END IF;
        END LOOP;

    IF is_currently_started AND task_start_time IS NOT NULL THEN
        current_duration := NOW() - task_start_time;
        task_total_duration := task_total_duration + current_duration;
    END IF;

    task_id := task_data."Id";
    task_name := task_data."Name";
    total_duration := task_total_duration;
    current_status := COALESCE(last_status, 'Created');
    is_started := (last_status = 'Started');
    is_paused := (last_status = 'Paused');
    is_stopped := (last_status = 'Stopped');
    developer_name := task_data.developer_name;
    start_time := first_start_time;
    update_time := last_update_time;
    sprint_id := task_data."SprintId"::UUID;
    team_id := task_data."TeamId"::UUID;
    manager_id := task_data."ManagerId"::UUID;

    RETURN NEXT;
END;
$$;

CREATE OR REPLACE FUNCTION CheckTaskBelongsToManager(
    p_task_id UUID,
    p_manager_id UUID
)
    RETURNS BOOLEAN
    LANGUAGE plpgsql
AS
$$
DECLARE
    task_count INTEGER;
BEGIN
    SELECT COUNT(*)
    INTO task_count
    FROM "Tasks" t
             INNER JOIN "Sprints" s ON t."SprintId" = s."Id"
    WHERE t."Id" = p_task_id
      AND s."ManagerId" = p_manager_id;

    RETURN task_count > 0;
END;
$$;

CREATE OR REPLACE FUNCTION GetManagerTeamAndSprint(
    p_manager_id UUID
)
    RETURNS TABLE
            (
                team_id   UUID,
                sprint_id UUID
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    manager_team_id  UUID;
    latest_sprint_id UUID;
BEGIN
    SELECT tm."Id"::UUID
    INTO manager_team_id
    FROM "Teams" tm
    WHERE tm."ManagerId" = p_manager_id;

    IF NOT FOUND THEN
        RETURN;
    END IF;

    SELECT s."Id"::UUID
    INTO latest_sprint_id
    FROM "Sprints" s
    WHERE s."TeamId" = manager_team_id
    ORDER BY s."StartDate" DESC
    LIMIT 1;

    team_id := manager_team_id;
    sprint_id := latest_sprint_id;

    RETURN NEXT;
END;
$$;