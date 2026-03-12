using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addpost_imagecolumntoposttabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Post_image_url",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Post_image_url",
                table: "posts");
        }
    }
}
