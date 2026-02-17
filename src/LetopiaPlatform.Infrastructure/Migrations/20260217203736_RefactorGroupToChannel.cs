using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorGroupToChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE user_communities SET role = 'Owner' WHERE role = 'Admin'");

            migrationBuilder.DropForeignKey(
                name: "FK_posts_groups_group_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropIndex(
                name: "ix_posts_group_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "posts");

            migrationBuilder.AddColumn<Guid>(
                name: "channel_id",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    community_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    channel_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Discussion"),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    post_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    allow_member_posts = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    allow_comments = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.id);
                    table.ForeignKey(
                        name: "FK_channels_channels_parent_id",
                        column: x => x.parent_id,
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_channels_communities_community_id",
                        column: x => x.community_id,
                        principalTable: "communities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_posts_channel_id",
                table: "posts",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_community_id",
                table: "channels",
                column: "community_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_community_parent_slug",
                table: "channels",
                columns: new[] { "community_id", "parent_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_channels_parent_id",
                table: "channels",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_posts_channels_channel_id",
                table: "posts",
                column: "channel_id",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE user_communities SET role = 'Admin' WHERE role = 'Owner';");
            
            migrationBuilder.DropForeignKey(
                name: "FK_posts_channels_channel_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "channels");

            migrationBuilder.DropIndex(
                name: "ix_posts_channel_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "channel_id",
                table: "posts");

            migrationBuilder.AddColumn<Guid>(
                name: "group_id",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    community_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    post_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_groups_communities_community_id",
                        column: x => x.community_id,
                        principalTable: "communities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_posts_group_id",
                table: "posts",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_community_slug",
                table: "groups",
                columns: new[] { "community_id", "slug" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_posts_groups_group_id",
                table: "posts",
                column: "group_id",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
