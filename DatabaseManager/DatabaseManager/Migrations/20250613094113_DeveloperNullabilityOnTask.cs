using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseManager.Migrations
{
    /// <inheritdoc />
    public partial class DeveloperNullabilityOnTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_DeveloperId",
                table: "Tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeveloperId",
                table: "Tasks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_DeveloperId",
                table: "Tasks",
                column: "DeveloperId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_DeveloperId",
                table: "Tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeveloperId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_DeveloperId",
                table: "Tasks",
                column: "DeveloperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
