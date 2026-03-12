using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addresoursestables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LikesCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityResources_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityResources_communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "communities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceLikes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResourceLikes_CommunityResources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "CommunityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_CommunityResources_CommunityId",
                table: "CommunityResources",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityResources_CreatedBy",
                table: "CommunityResources",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceLikes_ResourceId_UserId",
                table: "ResourceLikes",
                columns: new[] { "ResourceId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceLikes_UserId",
                table: "ResourceLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceTags_ResourceId_TagName",
                table: "ResourceTags",
                columns: new[] { "ResourceId", "TagName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceLikes");

            migrationBuilder.DropTable(
                name: "ResourceTags");

            migrationBuilder.DropTable(
                name: "CommunityResources");
        }
    }
}
