using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changesfieldConstraintsInUserRefreshTokenRepository : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_refresh_tokens_hash",
                table: "user_refresh_tokens");

            migrationBuilder.DropColumn(
                name: "token",
                table: "user_refresh_tokens");

            migrationBuilder.AlterColumn<string>(
                name: "refresh_token_hash",
                table: "user_refresh_tokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ix_user_refresh_tokens_hash",
                table: "user_refresh_tokens",
                column: "refresh_token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_refresh_tokens_hash",
                table: "user_refresh_tokens");

            migrationBuilder.AlterColumn<string>(
                name: "refresh_token_hash",
                table: "user_refresh_tokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "user_refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_refresh_tokens_hash",
                table: "user_refresh_tokens",
                column: "refresh_token_hash");
        }
    }
}
