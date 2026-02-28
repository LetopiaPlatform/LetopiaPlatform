using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addenumandprojectmemberandextrafields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "projects",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "difficulty_level",
                table: "projects",
                newName: "DifficultyLevel");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "projects",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Recruiting");

            migrationBuilder.AddColumn<List<string>>(
                name: "Goals",
                table: "projects",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercentage",
                table: "projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "projects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ProjectMember",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMember", x => new { x.ProjectId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_ProjectMember_AspNetUsers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMember_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_MemberId",
                table: "ProjectMember",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMember");

            migrationBuilder.DropColumn(
                name: "Goals",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "projects",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "DifficultyLevel",
                table: "projects",
                newName: "difficulty_level");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "projects",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Recruiting",
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
