using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcommunitytaskandcategoryanduserprogress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "community_task_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "#6366f1"),
                    icon_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    community_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_task_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_community_task_categories_communities_community_id",
                        column: x => x.community_id,
                        principalTable: "communities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "community_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
                    deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    community_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_community_tasks_communities_community_id",
                        column: x => x.community_id,
                        principalTable: "communities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_community_tasks_community_task_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "community_task_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_task_progress",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_task_progress", x => new { x.user_id, x.task_id });
                    table.ForeignKey(
                        name: "FK_user_task_progress_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_user_task_progress_community_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "community_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_community_task_categories_community_id",
                table: "community_task_categories",
                column: "community_id");

            migrationBuilder.CreateIndex(
                name: "ix_community_tasks_category_id",
                table: "community_tasks",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_community_tasks_community_id",
                table: "community_tasks",
                column: "community_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_task_progress_task_id",
                table: "user_task_progress",
                column: "task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_task_progress");

            migrationBuilder.DropTable(
                name: "community_tasks");

            migrationBuilder.DropTable(
                name: "community_task_categories");
        }
    }
}
