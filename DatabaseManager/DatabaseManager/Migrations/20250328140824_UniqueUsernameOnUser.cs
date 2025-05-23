using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseManager.Migrations
{
    /// <inheritdoc />
    public partial class UniqueUsernameOnUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Company_CompanyId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprint_Project_ProjectId",
                table: "Sprint");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprint_Users_ManagerId",
                table: "Sprint");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Sprint_SprintId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_TaskStatus_TaskStatusId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_TaskType_TaskTypeId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Users_DeveloperId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_TaskStatus_NewStatusId",
                table: "TaskHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_TaskStatus_OldStatusId",
                table: "TaskHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_Task_TaskId",
                table: "TaskHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskType",
                table: "TaskType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskStatus",
                table: "TaskStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskHistory",
                table: "TaskHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Task",
                table: "Task");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sprint",
                table: "Sprint");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Company",
                table: "Company");

            migrationBuilder.RenameTable(
                name: "TaskType",
                newName: "TaskTypes");

            migrationBuilder.RenameTable(
                name: "TaskStatus",
                newName: "TaskStatuses");

            migrationBuilder.RenameTable(
                name: "TaskHistory",
                newName: "TaskHistories");

            migrationBuilder.RenameTable(
                name: "Task",
                newName: "Tasks");

            migrationBuilder.RenameTable(
                name: "Sprint",
                newName: "Sprints");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.RenameTable(
                name: "Company",
                newName: "Companies");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistory_TaskId",
                table: "TaskHistories",
                newName: "IX_TaskHistories_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistory_OldStatusId",
                table: "TaskHistories",
                newName: "IX_TaskHistories_OldStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistory_NewStatusId",
                table: "TaskHistories",
                newName: "IX_TaskHistories_NewStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Task_TaskTypeId",
                table: "Tasks",
                newName: "IX_Tasks_TaskTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Task_TaskStatusId",
                table: "Tasks",
                newName: "IX_Tasks_TaskStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Task_SprintId",
                table: "Tasks",
                newName: "IX_Tasks_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_Task_DeveloperId",
                table: "Tasks",
                newName: "IX_Tasks_DeveloperId");

            migrationBuilder.RenameIndex(
                name: "IX_Sprint_ProjectId",
                table: "Sprints",
                newName: "IX_Sprints_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Sprint_ManagerId",
                table: "Sprints",
                newName: "IX_Sprints_ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Project_CompanyId",
                table: "Projects",
                newName: "IX_Projects_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskTypes",
                table: "TaskTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskStatuses",
                table: "TaskStatuses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskHistories",
                table: "TaskHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sprints",
                table: "Sprints",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Companies",
                table: "Companies",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Companies_CompanyId",
                table: "Projects",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Projects_ProjectId",
                table: "Sprints",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Users_ManagerId",
                table: "Sprints",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_TaskStatuses_NewStatusId",
                table: "TaskHistories",
                column: "NewStatusId",
                principalTable: "TaskStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_TaskStatuses_OldStatusId",
                table: "TaskHistories",
                column: "OldStatusId",
                principalTable: "TaskStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_Tasks_TaskId",
                table: "TaskHistories",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Sprints_SprintId",
                table: "Tasks",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskStatuses_TaskStatusId",
                table: "Tasks",
                column: "TaskStatusId",
                principalTable: "TaskStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskTypes_TaskTypeId",
                table: "Tasks",
                column: "TaskTypeId",
                principalTable: "TaskTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_DeveloperId",
                table: "Tasks",
                column: "DeveloperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Companies_CompanyId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Projects_ProjectId",
                table: "Sprints");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Users_ManagerId",
                table: "Sprints");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_TaskStatuses_NewStatusId",
                table: "TaskHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_TaskStatuses_OldStatusId",
                table: "TaskHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_Tasks_TaskId",
                table: "TaskHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Sprints_SprintId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskStatuses_TaskStatusId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskTypes_TaskTypeId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_DeveloperId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskTypes",
                table: "TaskTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskStatuses",
                table: "TaskStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskHistories",
                table: "TaskHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sprints",
                table: "Sprints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Companies",
                table: "Companies");

            migrationBuilder.RenameTable(
                name: "TaskTypes",
                newName: "TaskType");

            migrationBuilder.RenameTable(
                name: "TaskStatuses",
                newName: "TaskStatus");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "Task");

            migrationBuilder.RenameTable(
                name: "TaskHistories",
                newName: "TaskHistory");

            migrationBuilder.RenameTable(
                name: "Sprints",
                newName: "Sprint");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.RenameTable(
                name: "Companies",
                newName: "Company");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_TaskTypeId",
                table: "Task",
                newName: "IX_Task_TaskTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_TaskStatusId",
                table: "Task",
                newName: "IX_Task_TaskStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_SprintId",
                table: "Task",
                newName: "IX_Task_SprintId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_DeveloperId",
                table: "Task",
                newName: "IX_Task_DeveloperId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistories_TaskId",
                table: "TaskHistory",
                newName: "IX_TaskHistory_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistories_OldStatusId",
                table: "TaskHistory",
                newName: "IX_TaskHistory_OldStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistories_NewStatusId",
                table: "TaskHistory",
                newName: "IX_TaskHistory_NewStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Sprints_ProjectId",
                table: "Sprint",
                newName: "IX_Sprint_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Sprints_ManagerId",
                table: "Sprint",
                newName: "IX_Sprint_ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_CompanyId",
                table: "Project",
                newName: "IX_Project_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskType",
                table: "TaskType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskStatus",
                table: "TaskStatus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Task",
                table: "Task",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskHistory",
                table: "TaskHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sprint",
                table: "Sprint",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Company",
                table: "Company",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Company_CompanyId",
                table: "Project",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprint_Project_ProjectId",
                table: "Sprint",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sprint_Users_ManagerId",
                table: "Sprint",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Sprint_SprintId",
                table: "Task",
                column: "SprintId",
                principalTable: "Sprint",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_TaskStatus_TaskStatusId",
                table: "Task",
                column: "TaskStatusId",
                principalTable: "TaskStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_TaskType_TaskTypeId",
                table: "Task",
                column: "TaskTypeId",
                principalTable: "TaskType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Users_DeveloperId",
                table: "Task",
                column: "DeveloperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistory_TaskStatus_NewStatusId",
                table: "TaskHistory",
                column: "NewStatusId",
                principalTable: "TaskStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistory_TaskStatus_OldStatusId",
                table: "TaskHistory",
                column: "OldStatusId",
                principalTable: "TaskStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistory_Task_TaskId",
                table: "TaskHistory",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
