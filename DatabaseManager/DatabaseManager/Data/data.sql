BEGIN;

    DO $$
    BEGIN
        IF EXISTS (SELECT 1
            FROM public."Companies"
        UNION ALL
            SELECT 1
            FROM public."Projects"
        UNION ALL
            SELECT 1
            FROM public."Users"
        UNION ALL
            SELECT 1
            FROM public."TaskStatuses"
        UNION ALL
            SELECT 1
            FROM public."TaskTypes"
        UNION ALL
            SELECT 1
            FROM public."Sprints"
        UNION ALL
            SELECT 1
            FROM public."Tasks"
        UNION ALL
            SELECT 1
            FROM public."TaskHistories"
        UNION ALL
            SELECT 1
            FROM public."Teams"
        ) THEN
        TRUNCATE TABLE
                public."TaskHistories"
        ,
                public."Tasks",
                public."Sprints",
                public."Teams",
                public."Users",
                public."Projects",
                public."Companies",
                public."TaskStatuses",
                public."TaskTypes"
                RESTART IDENTITY CASCADE;
    END
    IF;
    END $$;

INSERT INTO public."Companies"
    ("Name")
VALUES
    ('Default Company'),
    ('Example Company');

INSERT INTO public."Projects"
    ("Id", "Name", "StartDate", "EndDate", "CompanyId")
VALUES
    ('7cb4ca9b-d493-4db5-b62d-89f4a737ad48', 'Project Alpha', '2025-01-01', '2025-06-30', (SELECT "Id"
        FROM public."Companies"
        WHERE "Name" = 'Example Company')),
    ('e68a0c61-874b-459a-b58e-42b0be1359a6', 'Project Beta', '2025-02-01', '2025-12-31', (SELECT "Id"
        FROM public."Companies"
        WHERE "Name" = 'Example Company')),
    ('f74408db-c52c-4c3c-94e4-16fdb1a0326b', 'Default', '0001-01-01', '9999-12-31', (SELECT "Id"
        FROM public."Companies"
        WHERE "Name" = 'Default Company'));

INSERT INTO public."Users"
    ("Id", "Username", "PasswordHash", "Role", "RefreshToken", "RefreshTokenExpiryTime", "Avatar", "PasswordSalt", "NeedResetPassword", "Email", "FirstName", "LastName")
VALUES
    ('0195e1ef-eda3-7e5f-9a9f-d6101c9b4644', 'admin', 'AQAAAAIAAYagAAAAEK5QXetwkM6E/ElyAX491XbOjzoq1v+3Q2NRXQEFanYGVL1O3zoHkzpWv6erfS1lww==', 'admin', NULL, NULL, NULL, 'zOnRFy+RiZ6TcHC8XPsMQQ==', false, 'admin@example.com', 'Ad', 'Min'),
    ('0195e20a-5055-7d77-a247-f093aded8758', 'manager1', 'AQAAAAIAAYagAAAAEGjuAs1ukAUbY+mLlcS+L4QT1FS40ATP597z3q9FvLTD813+EXV8SiAHhA8pzawJeg==', 'manager', NULL, NULL, NULL, 'WtWWjxcM/DRZIgBNAPr4xQ==', false, 'manager1@example.com', 'Mana', 'Ger1'),
    ('0195e20a-6f95-7713-9e4b-bb21508c9270', 'manager2', 'AQAAAAIAAYagAAAAEDieevQa4rEOonp5WjjRTJvU0JM0BfGIng2ZKgBvdv7Dnm2GOR06eRGm/vZmrBuoIA==', 'manager', NULL, NULL, NULL, 'y3hxZZWdBZ3n4M2RDJabfA==', false, 'manager2@example.com', 'Mana', 'Ger2');

INSERT INTO public."Teams"
    ("Id", "Name", "ManagerId", "ProjectId")
VALUES
    ('e2260dac-3823-41e7-a607-c6a94d19236b', 'Team 1', (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'manager1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('7a9871da-345c-4223-a7d4-1657f0dcdf5a', 'Team 2', (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'manager2'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta'));

INSERT INTO public."Users"
    ("Id", "Username", "PasswordHash", "Role", "RefreshToken", "RefreshTokenExpiryTime", "Avatar", "PasswordSalt", "TeamId", "NeedResetPassword", "Email", "FirstName", "LastName")
VALUES
    ('0195e20a-c2a2-7f9a-8719-e007bf497889', 'dev1', 'AQAAAAIAAYagAAAAECTGa9X4fItKgF1xmZzJSrHqtIBjEVRJVnZDpbDcjfEf0JG8+3YXV0k20cWH8ID9Nw==', 'developer', NULL, NULL, NULL, '5b1Qbyx8gqO0ymPhj5N1Ng==', (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 1'), false, 'dev1@example.com', 'Developer', 'Nr. 1'),
    ('0195e20a-d6c9-7cdc-ae6e-750db8a3b69f', 'dev2', 'AQAAAAIAAYagAAAAEKXOufPcJAyhhZNrqHsAjNXp1rthb6oHsOxOCVF+lfbM2dzqHTefJl9I/9sfY+ZrZw==', 'developer', NULL, NULL, NULL, 'FLvWhqaLp2DwxjWGAn/T7g==', (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 1'), false, 'dev2@example.com', 'Developer', 'Nr. 2'),
    ('0195e20a-ebc0-7b68-ba97-26bdd43964db', 'dev3', 'AQAAAAIAAYagAAAAEFGPxNh4HtKmngpGi9oMXKzMdDv09qumupRabxl6ZBfNdutlUtNhbIFH4D9bOtToNw==', 'developer', NULL, NULL, NULL, '9SiIrdY8AI0iPDLsyjk7ew==', (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 1'), false, 'dev3@example.com', 'Developer', 'Nr. 3'),
    ('0195e20a-fb84-747f-9e4d-356496149d8f', 'dev4', 'AQAAAAIAAYagAAAAEBHqTlyEpDoIx3KlnAT3IMm0iUpi79xsjr/+W7HhxVYukF8J6wZskiGpAkUc+/aSSg==', 'developer', NULL, NULL, NULL, 'zqKWb1TMJPK8KMDXV85ktg==', (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 1'), false, 'dev4@example.com', 'Developer', 'Nr. 4'),
    ('0195e20b-0b19-7715-86b9-77cd57423373', 'dev5', 'AQAAAAIAAYagAAAAEH7bF+BcWWKbVneacKuJuSADrBjOWYJ3RcXmpNfADf7S+wVokuKmRM4hiYJP9tRBBw==', 'developer', NULL, NULL, NULL, 'IcVJNuI1hmM0hSomqS5TpA==', (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 2'), false, 'dev5@example.com', 'Developer', 'Nr. 5'),
    ('0195e20b-1bfc-7c5b-889d-ac6238b88dd8', 'dev6', 'AQAAAAIAAYagAAAAELZhguZpaoDZPdTcQy6QRoyHC/q/XpttiE36DtsJDXXvcPN1VPOPqPXtTqiPNYHkIQ==', 'developer', NULL, NULL, NULL, 'Y4IINTwnpvspW1NrHN4J9g==', (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 2'), false, 'dev6@example.com', 'Developer', 'Nr. 6'),
    ('0195e20b-2bb7-77ba-80a5-b5d1364c0774', 'dev7', 'AQAAAAIAAYagAAAAEGLtTQr0QH+EEtRnEj0CKSfiNfyOF4kqEnBBEklwGdv9/hbFfxPTlbaIlcjfZc6XrQ==', 'developer', NULL, NULL, NULL, 'WBYSSIACTwF+YFl9LpVXpw==', (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 2'), false, 'dev7@example.com', 'Developer', 'Nr. 7');


INSERT INTO public."TaskStatuses"
    ("Name")
VALUES
    ('Created'),
    ('Assigned'),
    ('Started'),
    ('Paused'),
    ('Stopped'),
    ('Completed');


INSERT INTO public."TaskTypes"
    ("Name")
VALUES
    ('Feature'),
    ('Bug'),
    ('Improvement'),
    ('Research'),
    ('Task');


INSERT INTO public."Sprints"
    ("Id", "Name", "StartDate", "EndDate", "ManagerId", "ProjectId", "TeamId")
VALUES
    ('6407002c-e35a-4b18-a5a1-8a886d0c3b78', 'Sprint 1', '2025-01-10', '2025-01-24', (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'manager1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha'), (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 1')),
    ('ee5cc4ce-a7a9-4a78-9706-8658e2e72947', 'Sprint 2', '2025-02-05', '2025-02-19', (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'manager2'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta'), (SELECT "Id"
        FROM public."Teams"
        WHERE "Name" = 'Team 2'));

INSERT INTO public."Tasks"
    ("Id", "Name", "Description", "TaskTypeId", "TaskStatusId", "DeveloperId", "SprintId", "ProjectId")
VALUES
    ('df0190b9-6ccf-4982-af94-645840fc92b8', 'Task 1', 'Description for Task 1', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Feature'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev1'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('813f2be9-9f73-4acd-94e5-e2ae603efb23', 'Task 2', 'Description for Task 2', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Bug'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Assigned'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev2'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('5b1a4edd-fd0a-480c-af14-8a0063f277c1', 'Task 3', 'Description for Task 3', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Improvement'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Started'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev3'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('59edc273-6696-474c-854b-f1b7927bd3e9', 'Task 4', 'Description for Task 4', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Research'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Paused'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev4'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('410bd848-09ef-4442-97b4-03f3bfc3bd2b', 'Task 5', 'Description for Task 5', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Task'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Stopped'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev5'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('4e9f18d4-45f5-474b-86e7-570a6ee94c34', 'Task 6', 'Description for Task 6', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Feature'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Completed'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev6'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('62494fc4-4cf8-4ab0-b9f4-ce3201370d80', 'Task 7', 'Description for Task 7', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Bug'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Assigned'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev7'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('7d578d1a-d1dc-465a-af32-9846e375d79d', 'Task 8', 'Description for Task 8', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Improvement'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Started'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev1'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 1'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('f17c4006-6f6e-45c5-a2c9-d93e88af980b', 'Task 9', 'Description for Task 9', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Research'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Paused'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev2'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 2'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('9295bf35-fcf5-4d14-97fc-369bb71f9a3e', 'Task 10', 'Description for Task 10', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Task'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Stopped'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev3'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 2'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('8617f470-d577-4860-bd15-2d831cde6e53', 'Task 11', 'Description for Task 11', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Feature'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev4'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 2'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('92019240-9719-4488-9aca-dcfe488b28f0', 'Task 12', 'Description for Task 12', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Bug'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Assigned'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev5'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 2'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('6a58849a-9be7-4c78-89cb-96811e0a7857', 'Task 13', 'Description for Task 13', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Improvement'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Completed'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev6'), (SELECT "Id"
        FROM public."Sprints"
        WHERE "Name" = 'Sprint 2'), (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('cccc9bc9-e0d4-430a-ad9e-ef115a64101f', 'Task 14', 'Description for Task 14', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Bug'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Paused'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev7'), NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('64e5e60a-756a-49e8-8aa7-5877e4432aae', 'Task 15', 'Description for Task 15', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Task'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Stopped'), (SELECT "Id"
        FROM public."Users"
        WHERE "Username" = 'dev1'), NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('a1b2c3d4-e5f6-7890-1234-567890abcdef', 'Unassigned Task Alpha 1', 'Description for Unassigned Task Alpha 1', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Feature'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), NULL, NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('b2c3d4e5-f6a7-8901-2345-678901bcdef0', 'Unassigned Task Alpha 2', 'Description for Unassigned Task Alpha 2', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Bug'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), NULL, NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('c3d4e5f6-a7b8-9012-3456-789012cdef01', 'Unassigned Task Alpha 3', 'Description for Unassigned Task Alpha 3', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Improvement'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), NULL, NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Alpha')),
    ('d4e5f6a7-b8c9-0123-4567-890123def012', 'Unassigned Task Beta 1', 'Description for Unassigned Task Beta 1', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Research'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), NULL, NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('e5f6a7b8-c9d0-1234-5678-901234ef0123', 'Unassigned Task Beta 2', 'Description for Unassigned Task Beta 2', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Task'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), NULL, NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta')),
    ('f6a7b8c9-d0e1-2345-6789-012345f01234', 'Unassigned Task Beta 3', 'Description for Unassigned Task Beta 3', (SELECT "Id"
        FROM public."TaskTypes"
        WHERE "Name" = 'Feature'), (SELECT "Id"
        FROM public."TaskStatuses"
        WHERE "Name" = 'Created'), NULL, NULL, (SELECT "Id"
        FROM public."Projects"
        WHERE "Name" = 'Project Beta'));


INSERT INTO public."TaskHistories"
    ("Id", "TaskId", "ChangeDate", "NewStatus", "OldStatus")
VALUES
    ('1f1cd7a6-8297-4dbc-82d9-6d7caa440ead', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 1'), '2025-01-05 09:00:00', 'Created', ''),
    ('ffd33afd-f6b3-442d-ac2e-a1d4bdb6caaa', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 1'), '2025-01-06 10:00:00', 'Assigned', 'Created'),
    ('2636ae09-3bba-473b-8ee1-e641ad20576f', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 2'), '2025-01-07 11:00:00', 'Created', ''),
    ('5254c73f-463f-4f42-9e1f-51302a18f7f0', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 2'), '2025-01-08 12:00:00', 'Assigned', 'Created'),
    ('d259c8f5-68a8-4d42-9623-ee823e1ede2f', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 3'), '2025-01-09 13:00:00', 'Created', ''),
    ('09e99561-63b3-4b7a-8d28-bebe4baa670d', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 3'), '2025-01-10 14:00:00', 'Assigned', 'Created'),
    ('ff8846f3-13ec-41db-b10b-2c72a73fe446', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 4'), '2025-01-11 15:00:00', 'Created', ''),
    ('87791908-423b-4f7c-a380-015c20fdcbb7', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 4'), '2025-01-12 16:00:00', 'Assigned', 'Created'),
    ('a5491e99-b42a-446b-b3fa-a386c8010389', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 5'), '2025-01-13 17:00:00', 'Created', ''),
    ('153ee98b-ca3a-4470-a98a-9014160e3696', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 5'), '2025-01-14 18:00:00', 'Assigned', 'Created'),
    ('0873267e-b469-4ba3-8797-f0cd38f07745', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 6'), '2025-01-15 09:00:00', 'Created', ''),
    ('eea4ec88-0671-4a3b-85ed-3e8d38e79b2e', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 6'), '2025-01-16 10:00:00', 'Assigned', 'Created'),
    ('37b7cb51-0832-48fa-99c2-603fcb321725', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 7'), '2025-01-17 11:00:00', 'Created', ''),
    ('4fb510f5-8839-4697-ab5a-9ed3a3037466', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 7'), '2025-01-18 12:00:00', 'Assigned', 'Created'),
    ('9164cac1-cf32-4507-b272-0648ade1c7fa', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 8'), '2025-01-19 13:00:00', 'Created', ''),
    ('8b688e74-db6f-442a-9175-5a1ade9e47ed', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 8'), '2025-01-20 14:00:00', 'Assigned', 'Created'),
    ('f5a6e032-707d-4efd-8854-0bb3bfc64704', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 9'), '2025-01-21 15:00:00', 'Created', ''),
    ('fd2884a2-a804-4432-8463-223b9f521325', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 10'), '2025-01-22 16:00:00', 'Created', ''),
    ('e5877ff1-f08b-4cae-a6da-6ed4a8fafc76', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 11'), '2025-01-23 17:00:00', 'Created', ''),
    ('42371d27-9348-4470-b737-42a86720bc1c', (SELECT "Id"
        FROM public."Tasks"
        WHERE "Name" = 'Task 12'), '2025-01-24 18:00:00', 'Created', '');
