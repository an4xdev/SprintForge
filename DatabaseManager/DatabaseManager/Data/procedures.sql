CREATE OR REPLACE FUNCTION GetTaskDurations(
    p_sprint_id UUID
)
RETURNS TABLE (
    task_id UUID,
    task_name TEXT,
    total_duration INTERVAL,
    in_progress BOOLEAN
)
LANGUAGE plpgsql
AS $$
DECLARE
task_rec RECORD;
    history_rec RECORD;
    current_duration INTERVAL;
    task_total_duration INTERVAL;
    task_in_progress BOOLEAN;
    start_time TIMESTAMP;
    is_started BOOLEAN;
BEGIN
FOR task_rec IN
SELECT t."Id", t."Name"
FROM "Tasks" t
WHERE t."SprintId" = p_sprint_id
  AND t."TaskStatusId" NOT IN (1, 2)
ORDER BY t."Name"
    LOOP
        task_total_duration := INTERVAL '0';
task_in_progress := FALSE;
        start_time := NULL;
        is_started := FALSE;

FOR history_rec IN
SELECT th."NewStatus", th."ChangeDate"
FROM "TaskHistories" th
WHERE th."TaskId" = task_rec."Id"
ORDER BY th."ChangeDate"
    LOOP
            IF history_rec."NewStatus" = 'Started' THEN
                start_time := history_rec."ChangeDate";
is_started := TRUE;
                task_in_progress := TRUE;

            ELSIF history_rec."NewStatus" IN ('Stopped', 'Paused') AND is_started THEN
                IF start_time IS NOT NULL THEN
                    current_duration := history_rec."ChangeDate" - start_time;
                    task_total_duration := task_total_duration + current_duration;
END IF;
                is_started := FALSE;
                task_in_progress := FALSE;
                start_time := NULL;
END IF;
END LOOP;

        IF is_started AND start_time IS NOT NULL THEN
            current_duration := NOW() - start_time;
            task_total_duration := task_total_duration + current_duration;
            task_in_progress := TRUE;
END IF;

        task_id := task_rec."Id";
        task_name := task_rec."Name";
        total_duration := task_total_duration;
        in_progress := task_in_progress;

        RETURN NEXT;
END LOOP;

    RETURN;
END;
$$;