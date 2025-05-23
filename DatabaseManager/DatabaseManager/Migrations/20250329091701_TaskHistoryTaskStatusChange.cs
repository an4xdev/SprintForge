using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseManager.Migrations
{
    /// <inheritdoc />
    public partial class TaskHistoryTaskStatusChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_TaskStatuses_NewStatusId",
                table: "TaskHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_TaskStatuses_OldStatusId",
                table: "TaskHistories");

            migrationBuilder.DropIndex(
                name: "IX_TaskHistories_NewStatusId",
                table: "TaskHistories");

            migrationBuilder.DropIndex(
                name: "IX_TaskHistories_OldStatusId",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "NewStatusId",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "OldStatusId",
                table: "TaskHistories");

            migrationBuilder.AddColumn<string>(
                name: "NewStatus",
                table: "TaskHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldStatus",
                table: "TaskHistories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewStatus",
                table: "TaskHistories");

            migrationBuilder.DropColumn(
                name: "OldStatus",
                table: "TaskHistories");

            migrationBuilder.AddColumn<int>(
                name: "NewStatusId",
                table: "TaskHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OldStatusId",
                table: "TaskHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistories_NewStatusId",
                table: "TaskHistories",
                column: "NewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistories_OldStatusId",
                table: "TaskHistories",
                column: "OldStatusId");

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
        }
    }
}
