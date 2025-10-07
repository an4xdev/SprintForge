CREATE OR REPLACE FUNCTION GetManagerTasksWithDetails(
    p_manager_id UUID DEFAULT NULL
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
        WHERE (p_manager_id IS NULL OR s."ManagerId" = p_manager_id)
        ORDER BY t."Name"
        LOOP
            task_total_duration := INTERVAL '0';
            task_start_time := NULL;
            is_currently_started := FALSE;
            last_status := 'Created';
            last_update_time := CURRENT_TIMESTAMP;
            first_start_time := NULL;

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


                        IF first_start_time IS NULL THEN
                            first_start_time := history_rec."ChangeDate";
                        END IF;

                    ELSIF history_rec."NewStatus" IN ('Stopped', 'Paused', 'Completed') AND is_currently_started THEN
                        IF task_start_time IS NOT NULL THEN
                            current_duration := history_rec."ChangeDate" - task_start_time;
                            task_total_duration := task_total_duration + current_duration;
                        END IF;
                        is_currently_started := FALSE;
                        task_start_time := NULL;
                    END IF;
                END LOOP;


            IF is_currently_started AND task_start_time IS NOT NULL THEN
                current_duration := CURRENT_TIMESTAMP - task_start_time;
                task_total_duration := task_total_duration + current_duration;
            END IF;

            task_id := task_rec."Id";
            task_name := task_rec."Name";
            total_duration := task_total_duration;

            IF last_status = 'Created' AND last_update_time = CURRENT_TIMESTAMP THEN
                last_status := task_rec.initial_status;
            END IF;

            current_status := COALESCE(last_status, 'Created');
            is_started := (last_status = 'Started');
            is_paused := (last_status = 'Paused');
            is_stopped := (last_status = 'Stopped');


            IF last_status NOT IN ('Created', 'Assigned', 'Completed') THEN
                task_id := task_rec."Id";
                task_name := task_rec."Name";
                total_duration := task_total_duration;
                developer_name := task_rec.developer_name;
                start_time := first_start_time;
                update_time := last_update_time;
                sprint_id := task_rec."SprintId"::UUID;
                team_id := task_rec."TeamId"::UUID;

                RETURN NEXT;
            END IF;
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
    last_status := 'Created';
    last_update_time := CURRENT_TIMESTAMP;
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

            ELSIF history_rec."NewStatus" IN ('Stopped', 'Paused', 'Completed') AND is_currently_started THEN
                IF task_start_time IS NOT NULL THEN
                    current_duration := history_rec."ChangeDate" - task_start_time;
                    task_total_duration := task_total_duration + current_duration;
                END IF;
                is_currently_started := FALSE;
                task_start_time := NULL;
            END IF;
        END LOOP;

    IF is_currently_started AND task_start_time IS NOT NULL THEN
        current_duration := CURRENT_TIMESTAMP - task_start_time;
        task_total_duration := task_total_duration + current_duration;
    END IF;

    task_id := task_data."Id";
    task_name := task_data."Name";
    total_duration := task_total_duration;

    IF last_status = 'Created' AND last_update_time = CURRENT_TIMESTAMP THEN
        last_status := task_data.initial_status;
    END IF;

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
    p_manager_id UUID DEFAULT NULL
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
    team_record      RECORD;
BEGIN
    IF p_manager_id IS NOT NULL THEN

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
    ELSE

        FOR team_record IN
            SELECT tm."Id"::UUID as team_uuid
            FROM "Teams" tm
            LOOP
                SELECT s."Id"::UUID
                INTO latest_sprint_id
                FROM "Sprints" s
                WHERE s."TeamId" = team_record.team_uuid
                ORDER BY s."StartDate" DESC
                LIMIT 1;

                team_id := team_record.team_uuid;
                sprint_id := latest_sprint_id;

                RETURN NEXT;
            END LOOP;
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION GetSprintsReport(
    p_manager_id UUID DEFAULT NULL,
    p_start_date DATE DEFAULT NULL,
    p_end_date DATE DEFAULT NULL
)
    RETURNS TABLE
            (
                sprint_id            UUID,
                sprint_name          TEXT,
                task_count           INTEGER,
                task_count_completed INTEGER,
                total_task_time      INTERVAL,
                completed_ratio      NUMERIC
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    v_sprint_record        RECORD;
    v_task_count           INTEGER;
    v_task_count_completed INTEGER;
    v_total_task_time      INTERVAL;
    v_completed_ratio      NUMERIC;
    v_single_task_time     RECORD;
    v_single_task_id       UUID;
BEGIN

    FOR v_sprint_record IN
        SELECT s."Id", s."Name"
        FROM "Sprints" s
        WHERE (p_manager_id IS NULL OR s."ManagerId" = p_manager_id)
          AND (p_start_date IS NULL OR s."StartDate" >= p_start_date)
          AND (p_end_date IS NULL OR s."EndDate" <= p_end_date)
        LOOP
            SELECT COUNT(*)
            INTO v_task_count
            FROM "Tasks" t
            WHERE t."SprintId" = v_sprint_record."Id";

            SELECT COUNT(DISTINCT t."Id")
            INTO v_task_count_completed
            FROM "TaskHistories" th
                     INNER JOIN "Tasks" t ON th."TaskId" = t."Id"
            WHERE t."SprintId" = v_sprint_record."Id"
              AND th."NewStatus" = 'Completed';

            v_completed_ratio := NULL;
            IF v_task_count > 0 THEN
                v_completed_ratio := (v_task_count_completed::NUMERIC / v_task_count);
            END IF;

            v_total_task_time := INTERVAL '0';

            FOR v_single_task_id IN
                SELECT t."Id"
                FROM "Tasks" t
                WHERE t."SprintId" = v_sprint_record."Id"
                LOOP
                    SELECT * INTO v_single_task_time FROM GetSingleTaskDetails(v_single_task_id);
                    IF v_single_task_time.total_duration IS NOT NULL THEN
                        v_total_task_time := v_total_task_time + v_single_task_time.total_duration;
                    END IF;
                END LOOP;

            sprint_id := v_sprint_record."Id";
            sprint_name := v_sprint_record."Name";
            task_count := v_task_count;
            task_count_completed := v_task_count_completed;
            total_task_time := v_total_task_time;
            completed_ratio := v_completed_ratio;

            RETURN NEXT;

        END LOOP;
END;
$$;

CREATE OR REPLACE FUNCTION GetTeamsReport(
    p_manager_id UUID DEFAULT NULL,
    p_start_date DATE DEFAULT NULL,
    p_end_date DATE DEFAULT NULL
)
    RETURNS TABLE
            (
                developer_count      INTEGER,
                developer_ids        UUID[],
                sprints_names        TEXT[],
                task_count           INTEGER,
                task_count_completed INTEGER,
                total_task_time      INTERVAL
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    v_developer_count      INTEGER;
    v_developer_ids        UUID[];
    v_sprints_names        TEXT[];
    v_task_count           INTEGER;
    v_task_count_completed INTEGER;
    v_total_task_time      INTERVAL;
    v_manager_team_id      UUID;
    v_sprint               RECORD;
    v_task                 UUID;
BEGIN
    IF p_manager_id IS NOT NULL THEN
        SELECT t."Id" INTO v_manager_team_id FROM "Teams" t WHERE t."ManagerId" = p_manager_id;
        IF NOT FOUND THEN
            RETURN;
        END IF;
    END IF;

    SELECT ARRAY_AGG(DISTINCT u."Id")
    INTO v_developer_ids
    FROM "Users" u
             INNER JOIN "Tasks" ta ON u."Id" = ta."DeveloperId"
             INNER JOIN "Sprints" sp ON ta."SprintId" = sp."Id"
    WHERE (p_manager_id IS NULL OR sp."TeamId" = v_manager_team_id)
      AND (p_start_date IS NULL OR sp."StartDate" >= p_start_date)
      AND (p_end_date IS NULL OR sp."EndDate" <= p_end_date);

    v_developer_count := COALESCE(array_length(v_developer_ids, 1), 0);

    SELECT ARRAY_AGG(s."Name" ORDER BY s."StartDate" DESC)
    INTO v_sprints_names
    FROM "Sprints" s
    WHERE (p_manager_id IS NULL OR s."TeamId" = v_manager_team_id)
      AND (p_start_date IS NULL OR s."StartDate" >= p_start_date)
      AND (p_end_date IS NULL OR s."EndDate" <= p_end_date);

    SELECT COUNT(*)
    INTO v_task_count
    FROM "Tasks" t
             INNER JOIN "Sprints" s ON t."SprintId" = s."Id"
    WHERE (p_manager_id IS NULL OR s."TeamId" = v_manager_team_id)
      AND (p_start_date IS NULL OR s."StartDate" >= p_start_date)
      AND (p_end_date IS NULL OR s."EndDate" <= p_end_date);

    SELECT COUNT(DISTINCT t."Id")
    INTO v_task_count_completed
    FROM "TaskHistories" th
             INNER JOIN "Tasks" t ON th."TaskId" = t."Id"
             INNER JOIN "Sprints" s ON t."SprintId" = s."Id"
    WHERE (p_manager_id IS NULL OR s."TeamId" = v_manager_team_id)
      AND (p_start_date IS NULL OR s."StartDate" >= p_start_date)
      AND (p_end_date IS NULL OR s."EndDate" <= p_end_date)
      AND th."NewStatus" = 'Completed';

    v_total_task_time := INTERVAL '0';
    FOR v_sprint IN
        SELECT s."Id"
        FROM "Sprints" s
        WHERE (p_manager_id IS NULL OR s."TeamId" = v_manager_team_id)
          AND (p_start_date IS NULL OR s."StartDate" >= p_start_date)
          AND (p_end_date IS NULL OR s."EndDate" <= p_end_date)
        LOOP
            FOR v_task IN
                SELECT t."Id"
                FROM "Tasks" t
                WHERE t."SprintId" = v_sprint."Id"
                LOOP
                    DECLARE
                        v_task_time RECORD;
                    BEGIN
                        SELECT total_duration
                        INTO v_task_time
                        FROM GetSingleTaskDetails(v_task)
                        LIMIT 1;

                        IF v_task_time.total_duration IS NOT NULL THEN
                            v_total_task_time := v_total_task_time + v_task_time.total_duration;
                        END IF;
                    END;
                END LOOP;
        END LOOP;

    developer_count := v_developer_count;
    developer_ids := v_developer_ids;
    sprints_names := v_sprints_names;
    task_count := v_task_count;
    task_count_completed := v_task_count_completed;
    total_task_time := v_total_task_time;

    RETURN NEXT;
END;
$$;

CREATE OR REPLACE FUNCTION GetProjectsReport(
    p_manager_id UUID DEFAULT NULL,
    p_start_date DATE DEFAULT NULL,
    p_end_date DATE DEFAULT NULL
)
    RETURNS TABLE
            (
                project_id           UUID,
                project_name         TEXT,
                company_name         TEXT,
                sprint_count         INTEGER,
                task_count           INTEGER,
                task_count_completed INTEGER,
                total_task_time      INTERVAL,
                project_start_date   DATE,
                project_end_date     DATE,
                completed_ratio      NUMERIC
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    v_project_record       RECORD;
    v_sprint_count         INTEGER;
    v_task_count           INTEGER;
    v_task_count_completed INTEGER;
    v_total_task_time      INTERVAL;
    v_completed_ratio      NUMERIC;
    v_sprint               RECORD;
    v_task                 UUID;
BEGIN
    FOR v_project_record IN
        SELECT p."Id", p."Name", p."StartDate", p."EndDate", c."Name" as company_name
        FROM "Projects" p
                 INNER JOIN "Companies" c ON p."CompanyId" = c."Id"
        WHERE (p_start_date IS NULL OR p."StartDate" >= p_start_date)
          AND (p_end_date IS NULL OR p."EndDate" <= p_end_date)
        LOOP

            IF v_project_record."Name" = 'Default' THEN
                CONTINUE;
            END IF;

            SELECT COUNT(*)
            INTO v_sprint_count
            FROM "Sprints" s
            WHERE s."ProjectId" = v_project_record."Id"
              AND (p_manager_id IS NULL OR s."ManagerId" = p_manager_id);

            IF v_sprint_count = 0 THEN
                CONTINUE;
            END IF;


            SELECT COUNT(*)
            INTO v_task_count
            FROM "Tasks" t
                     INNER JOIN "Sprints" s ON t."SprintId" = s."Id"
            WHERE s."ProjectId" = v_project_record."Id"
              AND (p_manager_id IS NULL OR s."ManagerId" = p_manager_id);


            SELECT COUNT(DISTINCT t."Id")
            INTO v_task_count_completed
            FROM "TaskHistories" th
                     INNER JOIN "Tasks" t ON th."TaskId" = t."Id"
                     INNER JOIN "Sprints" s ON t."SprintId" = s."Id"
            WHERE s."ProjectId" = v_project_record."Id"
              AND (p_manager_id IS NULL OR s."ManagerId" = p_manager_id)
              AND th."NewStatus" = 'Completed';


            v_completed_ratio := NULL;
            IF v_task_count > 0 THEN
                v_completed_ratio := (v_task_count_completed::NUMERIC / v_task_count);
            END IF;


            v_total_task_time := INTERVAL '0';
            FOR v_sprint IN
                SELECT s."Id"
                FROM "Sprints" s
                WHERE s."ProjectId" = v_project_record."Id"
                  AND (p_manager_id IS NULL OR s."ManagerId" = p_manager_id)
                LOOP
                    FOR v_task IN
                        SELECT t."Id"
                        FROM "Tasks" t
                        WHERE t."SprintId" = v_sprint."Id"
                        LOOP
                            DECLARE
                                v_task_time RECORD;
                            BEGIN
                                SELECT total_duration
                                INTO v_task_time
                                FROM GetSingleTaskDetails(v_task)
                                LIMIT 1;

                                IF v_task_time.total_duration IS NOT NULL THEN
                                    v_total_task_time := v_total_task_time + v_task_time.total_duration;
                                END IF;
                            END;
                        END LOOP;
                END LOOP;

            project_id := v_project_record."Id";
            project_name := v_project_record."Name";
            company_name := v_project_record.company_name;
            sprint_count := v_sprint_count;
            task_count := v_task_count;
            task_count_completed := v_task_count_completed;
            total_task_time := v_total_task_time;
            project_start_date := v_project_record."StartDate";
            project_end_date := v_project_record."EndDate";
            completed_ratio := v_completed_ratio;

            RETURN NEXT;
        END LOOP;

    RETURN;
END;
$$;