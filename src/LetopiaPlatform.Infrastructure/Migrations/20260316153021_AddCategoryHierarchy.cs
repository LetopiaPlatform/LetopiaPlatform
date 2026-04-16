using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "parent_category_id",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_parent_category_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_parent_category_id",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "parent_category_id",
                table: "categories");
        }
    }
}
