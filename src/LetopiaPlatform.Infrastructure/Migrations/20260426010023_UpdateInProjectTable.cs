using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    public partial class UpdateInProjectTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_full",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "max_members",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "projects",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Goals",
                table: "projects",
                newName: "goals");

            migrationBuilder.RenameColumn(
                name: "DifficultyLevel",
                table: "projects",
                newName: "difficulty_level");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "projects",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "difficulty_level",
                table: "projects",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "timeline_events",
                table: "projects",
                type: "text[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "project_milestones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_milestones", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_milestones_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_milestones_project_id",
                table: "project_milestones",
                column: "project_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_milestones");

            migrationBuilder.DropColumn(
                name: "is_public",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "timeline_events",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "projects",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "goals",
                table: "projects",
                newName: "Goals");

            migrationBuilder.RenameColumn(
                name: "difficulty_level",
                table: "projects",
                newName: "DifficultyLevel");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "projects",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "projects",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "DifficultyLevel",
                table: "projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercentage",
                table: "projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_full",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "max_members",
                table: "projects",
                type: "integer",
                nullable: false,
                defaultValue: 5);
        }
    }
}
