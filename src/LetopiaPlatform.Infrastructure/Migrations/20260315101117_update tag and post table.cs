using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatetagandposttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_posts_AspNetUsers_author_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "ResourceTags");

            migrationBuilder.DropColumn(
                name: "Post_image_url",
                table: "posts");

            migrationBuilder.AddColumn<List<string>>(
                name: "image_urls",
                table: "posts",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TargetType_TargetId",
                table: "Tags",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TargetType_TargetId_TagName",
                table: "Tags",
                columns: new[] { "TargetType", "TargetId", "TagName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_posts_user_communities_author_id",
                table: "posts",
                column: "author_id",
                principalTable: "user_communities",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_posts_user_communities_author_id",
                table: "posts");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropColumn(
                name: "image_urls",
                table: "posts");

            migrationBuilder.AddColumn<string>(
                name: "Post_image_url",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResourceTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceTags_CommunityResources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "CommunityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResourceTags_ResourceId_TagName",
                table: "ResourceTags",
                columns: new[] { "ResourceId", "TagName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_posts_AspNetUsers_author_id",
                table: "posts",
                column: "author_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
